# SalonOS — Architecture for ASP.NET Core + Flutter

This maps the SalonOS backend architecture onto **ASP.NET Core (C#) + EF Core +
PostgreSQL** with a **Flutter** client. The conceptual design — modules, domain
flows, the four subsystems — is unchanged; see `SalonOS-Backend-Architecture.md`
for the *what*. This document is the *how* in .NET/Flutter, and spends its depth
on the few places where the stack genuinely changes the implementation or where
.NET has a sharp edge.

The `multi-tenancy` and `payments` rules still bind in full. They were written
with Prisma/TypeScript examples, but the rules are stack-agnostic: three
isolation layers, RLS as the floor, the mandatory isolation test, integer
minor-unit money, and provider-agnostic gateways. Below is their C# form.

---

## 1. Stack mapping

| Concept | NestJS/Prisma (original) | ASP.NET Core + Flutter |
|---|---|---|
| HTTP framework | NestJS | ASP.NET Core Web API (Minimal APIs or controllers) |
| ORM | Prisma | **EF Core 8** + Npgsql |
| DI | Nest providers | built-in `Microsoft.Extensions.DependencyInjection` |
| Module boundary | Nest module | C# class library project per bounded context |
| Tenant scoping (Layer 2) | manual `where tenantId` | **EF Core global query filters** |
| RLS session var | per-request `SET` | Npgsql + `SET LOCAL` in a tx (pooling-safe) |
| Money util | `packages/shared` | `Money` value object in a shared library |
| Payment abstraction | `PaymentProvider` interface | C# `IPaymentProvider`, region-selected via DI |
| Events / outbox | outbox table + worker | EF `SaveChanges` interceptor + Hangfire dispatcher |
| Background jobs | BullMQ/Redis | **Hangfire** (Postgres storage) or `BackgroundService` |
| Validation | class-validator | **FluentValidation** |
| Client | React Native | **Flutter** (Dio + Riverpod/Bloc) |
| SMS/push | provider interface | `INotificationProvider`, Kavenegar/SMS.ir adapter |

`long` replaces Prisma `BigInt` for money; `jsonb` (Npgsql) replaces Prisma
`Json`; `tstzrange` maps via `NpgsqlRange<DateTime>`.

---

## 2. Solution layout (modular monolith in .NET)

One deployable host, one DbContext, bounded contexts as projects. Modules
reference `Shared` and `Infrastructure`, never each other's internals — only
each other's **public service interface** project.

```
SalonOS.sln
├─ src/
│  ├─ SalonOS.Api/                 # host: middleware, DI wiring, endpoints
│  ├─ SalonOS.Shared/             # Money, Result, domain-event base, TenantContext
│  ├─ SalonOS.Infrastructure/     # AppDbContext, RLS, outbox, providers, Hangfire
│  ├─ Modules/
│  │  ├─ Identity/                # User, Tenant, Membership, auth, roles
│  │  ├─ Marketplace/             # ServiceTemplate (GLOBAL), licensing, purchase
│  │  ├─ Catalog/                 # CatalogService + options (TENANT)
│  │  ├─ Inventory/               # items + StockMovement ledger
│  │  ├─ Booking/                 # bookings + availability engine
│  │  ├─ Staff/                   # staff, skills, schedules
│  │  ├─ Reviews/                 # verified-booking ratings
│  │  ├─ Payroll/                 # periods, comp rules, adjustments, payslips
│  │  └─ Community/               # PUBLIC posts/comments/follows + leaderboard
│  │     └─ (Hiring seam)
└─ tests/
   └─ SalonOS.Tenancy.Tests/      # the mandatory cross-tenant isolation suite
```

Each `Modules/X` exposes `IXService` (e.g. `IBookingService`) in a small public
surface; consumers depend on the interface. Payroll calls
`IBookingService.ListCompletedForStaffAsync(...)` — it never touches the booking
tables. This is the C# form of the cross-module rule.

---

## 3. Tenancy in EF Core — the three layers

### Layer 1 — `TenantId` on every tenant-owned entity

A base type keeps it honest:

```csharp
public abstract class TenantEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }   // non-null, indexed (configured below)
}
```

```csharp
// In each entity config:
builder.HasIndex(x => x.TenantId);
```

Global entities (`User`, `Tenant`, `ServiceTemplate`, `TemplateOption…`,
`Membership`, and the PUBLIC community tables) do **not** derive from
`TenantEntity` and carry no `TenantId`. "If unsure, it's tenant-owned."

### Layer 2 — global query filters (the .NET idiom)

This is cleaner than manual scoping: register one filter per tenant entity and
**every** LINQ query is scoped automatically. The tenant comes from a
**scoped** `ITenantContext`, read from an instance field so EF re-evaluates it
per query (do NOT bake a constant in):

```csharp
public class AppDbContext : DbContext
{
    private readonly ITenantContext _tenant;
    public AppDbContext(DbContextOptions o, ITenantContext tenant) : base(o)
        => _tenant = tenant;

    protected override void OnModelCreating(ModelBuilder b)
    {
        // For every tenant-owned entity:
        b.Entity<Booking>().HasQueryFilter(x => x.TenantId == _tenant.TenantId);
        b.Entity<CatalogService>().HasQueryFilter(x => x.TenantId == _tenant.TenantId);
        // ... apply to all TenantEntity types (reflection loop recommended)
    }

    public override int SaveChanges() { StampTenant(); return base.SaveChanges(); }
    // also override SaveChangesAsync

    private void StampTenant()  // writes set TenantId from context, never payload
    {
        foreach (var e in ChangeTracker.Entries<TenantEntity>()
                     .Where(e => e.State == EntityState.Added))
            e.Entity.TenantId = _tenant.TenantId;
    }
}
```

`ITenantContext` is resolved by middleware from the authenticated token +
active membership — **never** from a request body/query param:

```csharp
public interface ITenantContext { Guid TenantId { get; } bool IsPlatformOwner { get; } }

// Middleware: validate token → look up Membership(userId, activeTenant)
//             → set a scoped TenantContext. Reject if no membership.
```

> .NET gotcha: a global query filter that references `_tenant.TenantId` is fine
> because it's an instance member (EF parameterizes it per query). Referencing a
> captured local/constant would bake the first tenant into the cached model.
> Also: EF warns when a filtered entity has a required relationship to another
> filtered entity — that's expected here; both sides are scoped to the same
> tenant. The platform-owner cross-tenant path uses `.IgnoreQueryFilters()` in
> one clearly named admin service only.

### Layer 3 — Postgres RLS (the floor), pooling-safe

RLS is the same SQL as before. The .NET-specific trap is **connection pooling**:
a plain `SET app.current_tenant` persists on the physical connection and leaks to
the next request that reuses it. Use `SET LOCAL` **inside a transaction** so the
setting dies with the transaction. Wire it with a command interceptor or a unit
of work that opens the tenant transaction:

```sql
ALTER TABLE "Booking" ENABLE ROW LEVEL SECURITY;
CREATE POLICY tenant_isolation ON "Booking"
  USING ("TenantId" = current_setting('app.current_tenant', true)::uuid);
```

```csharp
// At the start of each request's DB work, inside a transaction:
await using var tx = await db.Database.BeginTransactionAsync();
await db.Database.ExecuteSqlInterpolatedAsync(
    $"SET LOCAL app.current_tenant = {_tenant.TenantId.ToString()}");
// ... all reads/writes happen here ...
await tx.CommitAsync();
```

The `PLATFORM_OWNER` Postgres role is granted `BYPASSRLS` and is used only by the
one named cross-tenant admin/marketplace service.

### The mandatory isolation test (xUnit)

No module ships without it:

```csharp
[Fact]
public async Task TenantA_cannot_read_or_mutate_TenantB_booking()
{
    var (a, b) = await SeedTwoTenantsWithBookings();
    using var asA = ScopeFor(a);                 // TenantContext = A, SET LOCAL = A
    var leaked = await asA.Db.Bookings.FindAsync(b.BookingId);
    Assert.Null(leaked);                          // query filter + RLS → not found
    var mutate = () => asA.Service.CancelAsync(b.BookingId);
    await Assert.ThrowsAnyAsync<Exception>(mutate); // forbidden/not found, never B's data
}
```

---

## 4. Money in C#

Integer minor units + currency, never a float, all math through one shared type.
`long` covers IRR comfortably (max ~9.2×10¹⁸).

```csharp
public readonly record struct Money(long Amount, string Currency)
{
    public static Money Of(long amount, string ccy) => new(amount, ccy);

    public Money Add(Money o)      { Guard(o); return this with { Amount = Amount + o.Amount }; }
    public Money Subtract(Money o) { Guard(o); return this with { Amount = Amount - o.Amount }; }
    public Money Times(long qty)   => this with { Amount = Amount * qty };

    private void Guard(Money o)
    {
        if (o.Currency != Currency)
            throw new CurrencyMismatchException(Currency, o.Currency); // never coerce
    }
}
```

Persist as **two columns**, never one float. Map with an EF owned type or two
properties:

```csharp
builder.OwnsOne(x => x.Price, p => {
    p.Property(m => m.Amount).HasColumnName("PriceAmount").HasColumnType("bigint");
    p.Property(m => m.Currency).HasColumnName("PriceCurrency").HasColumnType("text");
});
```

Payroll amounts (base, commission, bonus, deduction, net) are all `Money` and run
through this type. Payroll math is correctness-critical — keep it covered by
tests; never let an approximate/local helper do money arithmetic.

---

## 5. Payments — `IPaymentProvider`, region-selected

Domain code depends only on the interface; gateway SDKs live in adapters in
`Infrastructure`. Active provider chosen by the salon's region via DI.

```csharp
public interface IPaymentProvider
{
    Task<PaymentSession> CreatePaymentAsync(CreatePaymentInput input, string idempotencyKey);
    Task<PaymentResult>  VerifyPaymentAsync(string reference);
    WebhookEvent         VerifyWebhook(ReadOnlySpan<byte> payload, string signature);
}

// Adapters: ZarinpalProvider (Iran now), StripeProvider (global later).
// Selection:
services.AddKeyedScoped<IPaymentProvider, ZarinpalProvider>("IR");
services.AddKeyedScoped<IPaymentProvider, StripeProvider>("GLOBAL");
// Resolve by tenant region at runtime via a small IPaymentProviderFactory.
```

Rules carried over verbatim: **idempotency key** on every charge (the three
flows are package purchase, optional booking deposit, payouts); **gateway state
is authoritative** — confirm on signature-verified, idempotent webhook + explicit
verify, never trust a redirect; webhook handlers must tolerate duplicate
delivery. Adding a provider = a new adapter + a config entry; it never touches
Booking/Catalog/Payroll.

---

## 6. Booking concurrency in Postgres via EF migration

The overlap guarantee is a **GiST exclusion constraint**, added with raw SQL in
an EF migration (EF can't model it directly). `tstzrange` maps to
`NpgsqlRange<DateTime>`:

```csharp
public DateTime StartsAt { get; set; }
public DateTime EndsAt   { get; set; }
public NpgsqlRange<DateTime> TimeRange { get; set; } // or a computed range column
```

```csharp
// Inside Up():
migrationBuilder.Sql("""
  CREATE EXTENSION IF NOT EXISTS btree_gist;
  ALTER TABLE "Booking" ADD CONSTRAINT no_overlap
    EXCLUDE USING gist (
      "StaffId"  WITH =,
      "TenantId" WITH =,
      "TimeRange" WITH &&
    ) WHERE (status IN ('CONFIRMED','IN_PROGRESS'));
""");
```

A short-lived **Redis** slot hold (StackExchange.Redis, key TTL) during checkout
is UX only; the exclusion constraint is the correctness floor. Catch the
constraint-violation `PostgresException` and surface a clean "slot just taken."

---

## 7. Catalog / Inventory / Community — EF specifics

- **Global vs tenant in EF:** `ServiceTemplate`, `TemplateOptionGroup`,
  `TemplateOption`, `PackageListing` are global entities — **no query filter**,
  no `TenantId`. `SalonPackageLicense`, `CatalogService`, `CatalogServiceOption`,
  `InventoryItem`, `StockMovement` are `TenantEntity` with filters + RLS.
- **Customer selection snapshot:** store the chosen options on `Booking` as
  `jsonb` (Npgsql `.HasColumnType("jsonb")`), plus a `Money` price snapshot, so a
  later catalog edit never rewrites history.
- **Inventory ledger:** `StockMovement` is append-only; `OnHandQty` is a cached
  projection. Decrement on `BookingCompleted`; when `OnHandQty ≤ ReorderThreshold`
  raise `InventoryLow`. Use `decimal` for quantities (not money).
- **Community is the PUBLIC exception:** `Post`, `Comment`, `Follow` carry
  `AuthorSalonId` for **authorization** (author-only mutation), not isolation —
  **no global query filter, no RLS tenant policy**. Mark them with a clear
  `[PublicData]`/namespace convention so no one assumes RLS protects them. Only a
  salon's opted-in aggregates (rating, completed-count) appear publicly; bookings
  / payroll / inventory never leak. `Leaderboard` is a read-only Hangfire-built
  projection. The Hiring seam (`JobPosting` public, `Application` tenant-private)
  is left in place, not built.

---

## 8. Events, outbox, jobs

**Domain events + transactional outbox via a SaveChanges interceptor** so an
event row commits in the same transaction as the state change:

```csharp
public class OutboxInterceptor : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(...)
    {
        var events = ctx.ChangeTracker.Entries<IHasDomainEvents>()
            .SelectMany(e => e.Entity.DequeueEvents());
        foreach (var ev in events)
            ctx.Set<OutboxMessage>().Add(OutboxMessage.From(ev)); // same tx
        return base.SavingChangesAsync(...);
    }
}
```

A **Hangfire** recurring job (Postgres storage — no extra infra) drains the
outbox and dispatches: `BookingCompleted` → inventory consume + payroll accrual
+ review eligibility + community stats; `ReviewCreated` → rating aggregates +
leaderboard; `InventoryLow` → notify; `PackagePurchased` → provision
`CatalogService`. Reminders, payroll period close, and leaderboard refresh are
also Hangfire recurring jobs. (A `BackgroundService` worker is the
dependency-free alternative if you'd rather not add Hangfire.)

---

## 9. Flutter client

The client just consumes the contract; the architecture above doesn't dictate
Flutter internals, but a few things must line up with the backend:

- **Clean layering:** `data` (Dio + retrofit-style API clients, repositories) →
  `domain` (entities, use cases) → `presentation` (Riverpod or Bloc). Keep money
  as integer minor units + currency in the model; format only in the widget.
- **Dio interceptors:** one for auth (bearer token refresh), one that sends the
  **active tenant** selector so the backend resolves `TenantContext` from
  token+membership (the client signals which membership is active; the server
  still authorizes it — never trust the client's tenant claim).
- **Jalali calendar + RTL:** use a Shamsi date picker (e.g. `shamsi_date` /
  `persian_datetime_picker`), `Directionality(textDirection: rtl)`, and `fa`
  localization via `flutter_localizations`. Booking slot times and payroll month
  labels render in Jalali; the wire format stays UTC ISO-8601.
- **Guest booking:** support the phone-only flow (OTP) without a full account,
  matching the backend's guest-customer model.
- **Two screens carry most of the product:** the **availability/booking** screen
  (search slots → pick staff → pick options rendered from the salon's enabled
  `CatalogServiceOption`s → confirm) and the **community feed**. Build those
  against the option schema so adding a new package type needs no client change.

---

## 10. Cross-cutting

- **Validation:** FluentValidation per request DTO.
- **i18n / calendar:** fa + en, RTL, Jalali month boundaries for payroll and
  reporting (Gregorian months will make monthly numbers subtly wrong). Store
  UTC; bucket/render per tenant locale.
- **Notifications:** `INotificationProvider` with a Kavenegar/SMS.ir adapter now,
  push + email later — same provider-agnostic pattern as payments.
- **Audit:** append-only log around every money operation, every platform-owner
  cross-tenant access (the RLS bypass), and every PUBLIC-layer mutation — exactly
  the boundary-crossing spots.
- **Files:** `IFileStorage` (S3-compatible / local), region-selectable.

---

## 11. Build order (unchanged)

1. Foundation: Identity/Access, tenant middleware + `ITenantContext`, EF query
   filters, RLS + `SET LOCAL`, the isolation test harness, `Money`,
   `IPaymentProvider` + Zarinpal, outbox + Hangfire.
2. Core sellable loop: Marketplace + Catalog (template → license → instance +
   options) → Inventory → Booking + availability.
3. People: Staff, Reviews, Payroll.
4. Network: Community (+ leaderboard).
5. Later: Hiring on the existing seam.

Every module ships with the cross-tenant isolation test (seed A and B, auth as A,
fail to read/mutate B). No test → not done.
