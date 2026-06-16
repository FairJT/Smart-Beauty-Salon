# Smart Salon (SalonOS) — Comprehensive Technical Report

> **Version:** 1.0 | **Date:** June 2026
> **Audience:** Computer Architecture Engineers, Full-Stack Developers
> **Scope:** Complete system architecture, backend (SalonOS + legacy SmartSalon), Flutter client, infrastructure, access control, and build roadmap

---

## Table of Contents

1. [Executive Summary](#1-executive-summary)
2. [System Architecture Overview](#2-system-architecture-overview)
3. [Docker Infrastructure & Deployment](#3-docker-infrastructure--deployment)
4. [Backend Architecture — SalonOS Modular Monolith](#4-backend-architecture--salonos-modular-monolith)
5. [Multi-Tenancy Architecture](#5-multi-tenancy-architecture)
6. [Access Control & Authorization](#6-access-control--authorization)
7. [Authentication & JWT Token Flow](#7-authentication--jwt-token-flow)
8. [Domain Deep-Dive](#8-domain-deep-dive)
9. [Event-Driven Architecture & Background Jobs](#9-event-driven-architecture--background-jobs)
10. [Legacy Backend — SmartSalon](#10-legacy-backend--smartsalon)
11. [Flutter Client Architecture](#11-flutter-client-architecture)
12. [Build Roadmap & Current Status](#12-build-roadmap--current-status)
13. [Key Engineering Decisions & Trade-offs](#13-key-engineering-decisions--trade-offs)
14. [File Index](#14-file-index)

---

## 1. Executive Summary

**Smart Salon (SalonOS)** is a multi-tenant SaaS platform for beauty salon management, designed for the Iranian market. It provides:

- **Salon management**: staff, services, inventory, bookings, finance
- **Client experience**: salon discovery, appointment booking, loyalty, reviews
- **Marketplace**: service templates that salons can license and customize
- **Artist portfolio**: service showcases, ratings, earnings visibility
- **Platform administration**: multi-tenant oversight, billing, audit

### Key Technical Characteristics

| Aspect | Choice |
|--------|--------|
| Backend | C# / ASP.NET Core 9, modular monolith |
| Database | SQL Server (Azure SQL Edge in dev) |
| ORM | EF Core 8 |
| Client | Flutter Web (RTL, Persian, Jalali calendar) |
| State Management | Riverpod |
| Auth | JWT with permission claims (30-min access token) |
| Multi-tenancy | 3-layer: TenantId columns + EF global query filters + SQL Server RLS |
| Deployment | Docker Compose (SQL Server + 2 APIs + Nginx/Flutter) |

### Current State

The system is in a **transition phase**: a legacy backend (`SmartSalon`, port 5015) coexists with a new modular monolith backend (`SalonOS`, port 5016). The Flutter client currently connects to the legacy backend; migration to SalonOS endpoints is on the roadmap.

Permission foundation (Phase 1) is complete. Authorization plumbing through tenant scoping (Phases 2–4) is partially implemented. Controller lockdown, RLS, and tests (Phases 6–9) remain.

---

## 2. System Architecture Overview

```
┌─────────────────────────────────────────────────────────────────────┐
│                        Browser (Client)                             │
│                   http://localhost:8081                             │
└─────────────────────┬───────────────────────────────────────────────┘
                      │
                      ▼
┌─────────────────────────────────────────────────────────────────────┐
│                    Nginx (:8081 → :80)                              │
│  ┌──────────────────────────────────────────────────────────────┐   │
│  │  location /api/  →  proxy_pass http://salonos-api:5016/api/ │   │
│  │  location /      →  try_files $uri $uri/ /index.html        │   │
│  │                    (Flutter SPA)                              │   │
│  └──────────────────────────────────────────────────────────────┘   │
└─────────────────────┬───────────────────────┬───────────────────────┘
                      │                       │
          ┌───────────▼──────────┐  ┌────────▼────────┐
          │   SalonOS API        │  │  SmartSalon API  │
          │   (:5016)            │  │  (:5015)         │
          │   Modular Monolith   │  │  Legacy MVC      │
          │   NEW                │  │  LEGACY          │
          └───────────┬──────────┘  └────────┬─────────┘
                      │                       │
                      └───────────┬───────────┘
                                  │
                      ┌───────────▼──────────┐
                      │   SQL Server         │
                      │   (:1433)            │
                      │   SmartSalonDB       │
                      │   SalonOSDB          │
                      └──────────────────────┘

          All services on Docker bridge network: salon-network
```

### Responsibilities by Service

| Service | Port | Role |
|---------|------|------|
| **SQL Server** | 1433 | Data persistence (2 databases: SmartSalonDB, SalonOSDB) |
| **SmartSalon** | 5015 | Legacy backend — full CRUD via Razor Pages + API controllers |
| **SalonOS** | 5016 | New backend — modular monolith with permission-based auth |
| **Flutter Web** | 8081 | Client app served by Nginx, proxies `/api/` to SalonOS |

---

## 3. Docker Infrastructure & Deployment

### docker-compose.yml Structure

```yaml
services:
  sqlserver:     # Azure SQL Edge, port 1433, health check via sqlcmd
  smartsalon:    # Legacy API, port 5015, Depends on sqlserver (healthy)
  flutter-web:   # Nginx + Flutter SPA, port 8081, reverse-proxies /api/ to salonos-api
  salonos-api:   # New modular monolith, port 5016, Depends on sqlserver (healthy)

volumes:
  sqlserver_data:   # Persistent database storage

networks:
  salon-network:    # Bridge network for all services
```

### Nginx Configuration (`nginx.conf`)

```nginx
server {
    listen 80;
    server_name localhost;
    root /usr/share/nginx/html;
    index index.html;

    # API proxy to SalonOS backend
    location /api/ {
        proxy_pass http://salonos-api:5016/api/;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_read_timeout 90s;
    }

    # SPA fallback
    location / {
        try_files $uri $uri/ /index.html;
    }
}
```

### Health Checks

| Service | Check | Interval | Retries |
|---------|-------|----------|---------|
| SQL Server | `sqlcmd -Q "SELECT 1"` | 10s | 10 (30s start period) |
| SmartSalon | `curl /swagger/index.html` | 15s | 5 (20s start period) |
| SalonOS | `curl /swagger/index.html` | 15s | 5 (20s start period) |
| Flutter Web | `wget /` | 15s | 3 |

---

## 4. Backend Architecture — SalonOS Modular Monolith

### Solution Layout

```
SalonOS.sln
├── src/
│   ├── SalonOS.Api/              # Host: middleware pipeline, DI wiring, endpoint registration
│   ├── SalonOS.Shared/           # Shared kernel: Money, Result, TenantEntity, authorization, identity
│   ├── SalonOS.Infrastructure/   # AppDbContext, RLS interceptor, outbox, providers, background jobs
│   └── Modules/
│       ├── Identity/             # User, Tenant, Membership, auth service, roles
│       ├── Marketplace/          # ServiceTemplate (GLOBAL), licensing, purchase
│       ├── Booking/              # Bookings, availability engine, domain events
│       ├── Catalog/              # CatalogService + options (TENANT-scoped)
│       ├── Inventory/            # Items + StockMovement ledger
│       ├── Staff/                # Staff, skills, schedules
│       ├── Reviews/              # Verified-booking ratings
│       ├── Payroll/              # Periods, comp rules, adjustments, payslips
│       └── Community/            # PUBLIC posts/comments/follows + leaderboard
└── tests/
    └── SalonOS.Tenancy.Tests/    # Cross-tenant isolation test suite
```

### Module Boundary Pattern

Each module exposes a **public service interface** (e.g., `IBookingService`). Other modules depend only on the interface, never on internals. Cross-module communication goes through interfaces, never direct table access.

Example: Payroll calls `IBookingService.ListCompletedForStaffAsync(...)` — it never touches the Booking tables directly.

### Host Project: SalonOS.Api

**File:** `src/SalonOS.Api/Program.cs` (not shown in codebase — to be created during Phase 2)

The host wires:
1. Middleware pipeline: `UseAuthentication()` → `TenantResolutionMiddleware` → `UseAuthorization()`
2. DI registrations: `IAuthorizationPolicyProvider`, `IAuthorizationHandler`, `ICurrentUser`, `ITenantContext`, module services
3. Endpoint registration: controllers or minimal APIs per module

---

## 5. Multi-Tenancy Architecture

Multi-tenancy is the **core engineering challenge** of SalonOS. Three isolation layers work together:

### Layer 1 — TenantId on Every Tenant Entity

**File:** `src/SalonOS.Shared/MultiTenancy/TenantEntity.cs`

```csharp
public abstract class TenantEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }   // non-null, indexed
}
```

All tenant-owned entities (Booking, CatalogService, InventoryItem, etc.) inherit from `TenantEntity`. Global entities (User, Tenant, ServiceTemplate, Membership, Community tables) do **not** carry TenantId.

### Layer 2 — EF Core Global Query Filters

**File:** `src/SalonOS.Infrastructure/Persistence/AppDbContext.cs` (to be created during Phase 4)

Every LINQ query is automatically scoped. The tenant comes from a **scoped** `ITenantContext` instance field so EF re-evaluates it per query:

```csharp
protected override void OnModelCreating(ModelBuilder b)
{
    foreach (var et in b.Model.GetEntityTypes()
                 .Where(t => typeof(TenantEntity).IsAssignableFrom(t.ClrType)))
    {
        b.Entity(et.ClrType).HasIndex(nameof(TenantEntity.TenantId));
        // Global query filter: e.TenantId == _tenant.TenantId
        var p = Expression.Parameter(et.ClrType, "e");
        var body = Expression.Equal(
            Expression.Property(p, nameof(TenantEntity.TenantId)),
            Expression.Property(Expression.Constant(_tenant), nameof(ITenantContext.TenantId)));
        b.Entity(et.ClrType).HasQueryFilter(Expression.Lambda(body, p));
    }
}
```

Writes stamp TenantId from context, never from DTO:

```csharp
private void StampTenant()
{
    foreach (var e in ChangeTracker.Entries<TenantEntity>()
                 .Where(e => e.State == EntityState.Added))
        e.Entity.TenantId = _tenant.TenantId;
}
```

### Layer 3 — SQL Server Row-Level Security (the floor)

**File:** `src/SalonOS.Infrastructure/Interceptors/TenantSessionContextInterceptor.cs`

An EF Core connection interceptor runs `sp_set_session_context` immediately after each connection opens:

```sql
EXEC sp_set_session_context N'TenantId', @tenantId;
EXEC sp_set_session_context N'IsPlatformOwner', @isPlatformOwner;
```

The RLS security policy on each tenant table:

```sql
CREATE FUNCTION Security.fn_tenant(@TenantId uniqueidentifier)
RETURNS TABLE WITH SCHEMABINDING
AS RETURN
  SELECT 1 AS ok
  WHERE @TenantId = CAST(SESSION_CONTEXT(N'TenantId') AS uniqueidentifier)
     OR CAST(SESSION_CONTEXT(N'IsPlatformOwner') AS bit) = 1;

CREATE SECURITY POLICY Security.TenantFilter
  ADD FILTER PREDICATE Security.fn_tenant(TenantId) ON dbo.Bookings,
  ADD BLOCK  PREDICATE Security.fn_tenant(TenantId) ON dbo.Bookings AFTER INSERT
  WITH (STATE = ON);
```

### Tenant Resolution Chain

```
JWT Token
  └── "tenant_id" claim
        └── ICurrentUser.TenantId  (reads from HttpContext.User claims)
              └── ITenantContext.TenantId  (wraps ICurrentUser)
                    ├── EF Global Query Filters (Layer 2)
                    └── sp_set_session_context (Layer 3)
```

**CRITICAL RULE (R3):** Tenant id ALWAYS comes from the validated JWT claim. It is NEVER read from request body, query string, or route values.

### The Single Sanctioned Cross-Tenant Path

**File:** `src/SalonOS.Infrastructure/Admin/PlatformAdminService.cs`

Only `PlatformAdminService` may call `IgnoreQueryFilters()`. It is guarded at the controller level by `[HasPermission("tenant.manage")]` — PlatformOwner only. No other file in the codebase may call `IgnoreQueryFilters()`.

---

## 6. Access Control & Authorization

### The Three-Check Model

```
ACCESS GRANTED  ==  PERMISSION  ∧  TENANT  ∧  OWNERSHIP
                    (what you    (whose     (is this
                     can do)      salon)     row yours)
```

### Role Hierarchy

```
PLATFORM SCOPE  (crosses tenants — the only sanctioned cross-tenant path)
└── PlatformOwner     (SuperAdmin) — runs the SaaS, all salons, marketplace, billing

TENANT SCOPE  (one salon = one tenant)
├── SalonManager      (salon owner/admin) — full control of *their* salon
├── Receptionist      (front desk) — booking + desk payments, no admin
├── Artist            (stylist/staff) — own schedule, own performance, own payout
└── Client            (consumer) — own bookings, own loyalty, public browsing
```

Roles are NOT nested. Each is a distinct permission set, not inheritance.

### Permission Catalog (77 constants)

**File:** `src/SalonOS.Shared/Authorization/Permissions.cs`

| Resource | Permissions |
|----------|-------------|
| Salon | `salon.view`, `salon.edit`, `salon.settings.manage` |
| Staff / Artist | `staff.view`, `staff.create`, `staff.edit`, `staff.delete`, `staff.contract.manage`, `staff.performance.view` |
| Catalog / Service | `catalog.view`, `catalog.create`, `catalog.edit`, `catalog.delete`, `catalog.package.manage` |
| Appointment | `appointment.view.all`, `appointment.view.own`, `appointment.create`, `appointment.confirm`, `appointment.complete`, `appointment.cancel.all`, `appointment.cancel.own`, `appointment.rate` |
| Inventory | `inventory.view`, `inventory.adjust`, `inventory.manage` |
| Finance | `finance.revenue.view`, `finance.deposit.take`, `finance.payout.view.own`, `finance.payout.manage`, `finance.period.close` |
| Reports | `report.salon.view`, `report.staff.view.own`, `report.platform.view` |
| Loyalty | `loyalty.config.manage`, `loyalty.view.own` |
| Client | `client.self` |
| Notification | `notification.send`, `notification.view.own` |
| Marketplace | `marketplace.browse`, `marketplace.license.purchase`, `marketplace.template.manage` |
| Platform / Tenant | `tenant.manage`, `tenant.billing.manage`, `platform.config.manage`, `platform.audit.view` |

### Role → Permissions Map

**File:** `src/SalonOS.Shared/Authorization/RolePermissions.cs`

```csharp
public static readonly Dictionary<string, string[]> Map = new()
{
    ["SalonManager"] = new[] { /* 30 permissions — full salon control */ },
    ["Receptionist"] = new[] { /* 12 permissions — booking + deposit, no admin */ },
    ["Artist"]       = new[] { /* 9 permissions — own-scope only */ },
    ["Client"]       = new[] { /* 5 permissions — self-service only */ },
    // PlatformOwner is NOT in the map — uses bypass (see §6.4)
};
```

### Enforcement Code

**File:** `src/SalonOS.Shared/Authorization/PermissionRequirement.cs`
**File:** `src/SalonOS.Shared/Authorization/PermissionHandler.cs`
**File:** `src/SalonOS.Shared/Authorization/PermissionPolicyProvider.cs`
**File:** `src/SalonOS.Shared/Authorization/HasPermissionAttribute.cs`

```csharp
// Requirement
public sealed class PermissionRequirement(string permission) : IAuthorizationRequirement
{
    public string Permission { get; } = permission;
}

// Handler — checks JWT "permission" claims
public sealed class PermissionHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        if (context.User.HasClaim("permission", requirement.Permission))
            context.Succeed(requirement);
        return Task.CompletedTask;
    }
}

// Policy provider — maps "perm:xxx" policy names to requirements
public sealed class PermissionPolicyProvider : IAuthorizationPolicyProvider { /* ... */ }

// Attribute — usage on controllers
public sealed class HasPermissionAttribute(string permission)
    : AuthorizeAttribute($"perm:{permission}");
```

**Usage on a controller action:**

```csharp
[HttpPost("{id}/cancel")]
[HasPermission("appointment.cancel.all")]
public Task<IActionResult> Cancel(Guid id) { /* ... */ }
```

### Ownership Checks (IDOR Prevention)

**File:** `src/SalonOS.Api/Authorization/OwnsAppointment.cs`

For "own"-scoped actions, after loading the entity:

```csharp
public sealed class OwnsAppointmentHandler(ICurrentUser user)
    : AuthorizationHandler<OwnsAppointment, Booking>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext ctx, OwnsAppointment req, Booking appointment)
    {
        var owns = user.Role switch
        {
            "Artist" => user.ArtistId.HasValue && appointment.ArtistId == user.ArtistId.Value,
            "Client" => appointment.ClientId == user.UserId,
            _        => false
        };
        if (owns) ctx.Succeed(req);
        return Task.CompletedTask;
    }
}
```

**Usage in controller:**

```csharp
await _authz.AuthorizeAsync(User, appointment, new OwnsAppointment());
```

### Artist Contract Types & Financial Visibility

| Contract | Persian | Revenue Visible to Artist |
|----------|---------|--------------------------|
| Salaried | حقوق ثابت | Only own ratings & completed count. NO revenue figures. |
| Chair rental | اجاره خط | Own service revenue and own deposits. |
| Room rental | اجاره اتاق | Same as chair rental, plus their room's utilization. |

One permission (`finance.payout.view.own`), contract-aware response shaping — not separate roles.

---

## 7. Authentication & JWT Token Flow

### Registration Flow

```
Client                    API                     Database
  │   POST /register       │                         │
  │   {mobile, password,   │                         │
  │    firstName, lastName, │   CreateUserAsync()     │
  │    nationalCode}       │ ───────────────────────► │
  │                        │                     ASP.NET Identity
  │                        │   Create ClientProfile   │
  │                        │ ───────────────────────► │
  │                        │                         │
  │                        │   BuildTokenAsync()     │
  │                        │   (no tenant/permissions│
  │                        │    — client has no      │
  │                        │    membership yet)       │
  │   {token, expiresIn,   │                         │
  │    user}               │                         │
  │ ◄──────────────────── │                         │
```

### Login Flow

```
Client                    API                     Database
  │   POST /login          │                         │
  │   {mobile, password}   │   FindByNameAsync()     │
  │                        │ ───────────────────────► │
  │                        │   CheckPasswordAsync()  │
  │                        │ ───────────────────────► │
  │                        │                         │
  │                        │   Query Membership      │
  │                        │   (active, by userId)   │
  │                        │ ───────────────────────► │
  │                        │                         │
  │                        │   BuildTokenAsync():    │
  │                        │   - sub (userId)        │
  │                        │   - role                │
  │                        │   - tenant_id           │
  │                        │   - permission × N      │
  │                        │   - artist_id (if Artist)│
  │                        │   - is_platform_owner   │
  │                        │     (if SuperAdmin)     │
  │                        │                         │
  │   {token, expiresIn,   │                         │
  │    user}               │                         │
  │ ◄──────────────────── │                         │
```

### JWT Token Structure

**File:** `src/Modules/Identity/Infrastructure/AuthService.cs`

```json
{
  "sub": "user-guid",
  "name": "09123456789",
  "role": "SalonManager",
  "tenant_id": "tenant-guid",
  "permission": ["salon.view", "salon.edit", "staff.create", ...],
  "artist_id": null,
  "is_platform_owner": null,
  "iss": "SalonOS",
  "aud": "SalonOS",
  "exp": "2026-06-15T11:58:59Z"
}
```

**Token lifetime:** 30 minutes (security fix from original 30-day token — see §R6.2 of access control design).

### Flutter JWT Decoding

**File:** `smart_salon_app/lib/core/jwt_decoder.dart`

Client-side decoding is for **UX gating only** (§R8). The server enforces every rule independently.

```dart
class JwtDecoder {
  static Map<String, dynamic>? decode(String token) { /* base64url decode */ }
  static Set<String> extractPermissions(String token) { /* permission claims → Set */ }
  static bool isPlatformOwner(String token) { /* is_platform_owner == "true" */ }
  static String? role(String token) { /* role claim */ }
}
```

---

## 8. Domain Deep-Dive

### Identity Module

**Files:**
- `src/Modules/Identity/Domain/ApplicationUser.cs`
- `src/Modules/Identity/Domain/Tenant.cs`
- `src/Modules/Identity/Domain/Membership.cs`
- `src/Modules/Identity/Domain/Enums/UserType.cs`
- `src/Modules/Identity/Domain/Enums/MembershipRole.cs`
- `src/Modules/Identity/Infrastructure/IdentityDbContext.cs`
- `src/Modules/Identity/Infrastructure/AuthService.cs`

**Key entities:**

```csharp
public class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string NationalCode { get; set; }
    public UserType UserType { get; set; }          // SuperAdmin, SalonManager, Artist, Client
    public int LoyaltyPoints { get; set; }
    public int TotalVisits { get; set; }
    public bool IsActive { get; set; }
}

public class Tenant
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Slug { get; set; }
    public string LogoUrl { get; set; }
    public bool IsActive { get; set; }
    public string OwnerId { get; set; }             // ApplicationUser FK
}

public class Membership
{
    public Guid Id { get; set; }
    public string UserId { get; set; }
    public Guid TenantId { get; set; }
    public MembershipRole Role { get; set; }        // SalonManager, Receptionist, Artist, Client
    public bool IsActive { get; set; }
}
```

**UserType vs MembershipRole:** `UserType` is the user's global type (what they are). `MembershipRole` is their role within a specific tenant (what they can do there). A user can have multiple memberships across tenants.

### Booking Module

**Files:**
- `src/Modules/Booking/Domain/Booking.cs`
- `src/Modules/Booking/Domain/BookingStatus.cs`
- `src/Modules/Booking/Domain/Events/BookingCompleted.cs`
- `src/Modules/Booking/Domain/Events/BookingCancelled.cs`
- `src/Modules/Booking/Infrastructure/BookingDbContext.cs`

```csharp
public class Booking : TenantEntity
{
    public string ClientId { get; set; }
    public Guid ArtistId { get; set; }
    public Guid ServiceId { get; set; }
    public DateTime StartsAt { get; set; }
    public DateTime EndsAt { get; set; }
    public BookingStatus Status { get; set; }
    public bool ReminderSent { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}

public enum BookingStatus
{
    Pending, Confirmed, InProgress, Completed, Cancelled, NoShow
}
```

**Domain events** are raised on state transitions and captured by the outbox pattern (see §9).

### Catalog Module

**Files:**
- `src/Modules/Catalog/Domain/CatalogService.cs`
- `src/Modules/Catalog/Domain/ServiceOption.cs`
- `src/Modules/Catalog/Domain/Material.cs`

```csharp
public class CatalogService : TenantEntity
{
    public string Name { get; set; }
    public string Description { get; set; }
    public long PriceAmount { get; set; }
    public string PriceCurrency { get; set; }
    public int DurationMinutes { get; set; }
    public string ImageUrl { get; set; }
    public bool IsActive { get; set; }
    public List<ServiceOption> Options { get; set; }
    public List<Material> Materials { get; set; }
}
```

### Money Value Object

**File:** `src/SalonOS.Shared/Money.cs`

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
            throw new CurrencyMismatchException(Currency, o.Currency);
    }
}
```

Integer minor units (e.g., toman × 10), never float. `long` covers IRR comfortably (max ~9.2×10¹⁸).

---

## 9. Event-Driven Architecture & Background Jobs

### Transactional Outbox Pattern

Domain events are captured during `SaveChanges` and written to the `OutboxMessage` table in the **same transaction** as the state change. A background job drains the outbox and dispatches to handlers.

```
┌──────────────┐     ┌──────────────┐     ┌──────────────────┐
│  Domain      │     │  Outbox      │     │  Event Handlers  │
│  Event Raised│────►│  Message     │────►│                  │
│  (in-memory) │     │  (persisted) │     │  BookingCompleted│
│              │     │              │     │  BookingCancelled│
│  SaveChanges │     │  Outbox      │     │  InventoryLow    │
│  Interceptor │     │  Dispatcher  │     │  ReviewCreated   │
└──────────────┘     │  Job         │     │  PackagePurchased│
                     └──────────────┘     └──────────────────┘
```

### OutboxDispatcherJob

**File:** `src/SalonOS.Infrastructure/Jobs/OutboxDispatcherJob.cs`

```csharp
public class OutboxDispatcherJob
{
    // Polls up to 50 unprocessed messages, ordered by CreatedAt
    // Dispatches via handler map: eventType → handlerType
    // On success: sets ProcessedAt
    // On failure: increments RetryCount, stores error message
}
```

### Event Handlers (Stubs)

**File:** `src/SalonOS.Infrastructure/EventHandlers/BookingCompletedHandler.cs`
**File:** `src/SalonOS.Infrastructure/EventHandlers/BookingCancelledHandler.cs`

Both handlers are currently **stubs with TODOs**:

- `BookingCompletedHandler`: should consume inventory, mark review-eligible, update community stats, update artist rating
- `BookingCancelledHandler`: should restore inventory, send cancellation notification, update community stats

### ReminderJob

**File:** `src/SalonOS.Infrastructure/Jobs/ReminderJob.cs`

```csharp
public class ReminderJob
{
    // Finds confirmed bookings within 2 hours that haven't had reminders sent
    // Marks ReminderSent = true
    // Actual SMS/push notification sending is TODO
}
```

---

## 10. Legacy Backend — SmartSalon

### Architecture

**Location:** `SmartSalon/SmartSalon/`

Traditional ASP.NET MVC with Controllers → Services → EF Core models. This is the **legacy** backend running on port 5015.

### Controllers

| Controller | Key Endpoints |
|-----------|---------------|
| `SalonsController` | CRUD for salons, search, nearby |
| `ArtistsController` | CRUD for artists, portfolio, schedule |
| `ServicesController` | CRUD for services, options, pricing |
| `AppointmentsController` | Book, confirm, complete, cancel, availability |
| `NotificationsController` | Send, list, mark-read |
| `ReviewsController` | Create, list (verified-booking only) |
| `SearchController` | Full-text search across salons/artists/services |
| `UsersController` | Profile, loyalty, visit history |

### Validation

**Location:** `SmartSalon/SmartSalon/Validators/`

FluentValidation validators per DTO (e.g., `CreateAppointmentValidator`, `RegisterUserValidator`).

### DTOs

**Location:** `SmartSalon/SmartSalon/DTOs/`

Separate request and response DTOs per entity. AutoMapper profiles map between entities and DTOs.

### Transition Context

The Flutter client currently connects to SmartSalon (:5015). The migration plan is to:
1. Complete SalonOS backend (Phases 2–9 of access control)
2. Point Flutter's `ApiService.baseUrl` to `/api` (which Nginx proxies to SalonOS :5016)
3. Eventually decommission SmartSalon

---

## 11. Flutter Client Architecture

### Stack

| Layer | Technology |
|-------|------------|
| Framework | Flutter Web (Material 3) |
| State Management | Riverpod (StateNotifierProvider, FutureProvider) |
| HTTP | `http` package (ApiService), Dio planned |
| Local Storage | `flutter_secure_storage` (tokens), SharedPreferences |
| Navigation | GoRouter (route guards by role) |
| RTL | `Directionality(textDirection: RTL)`, Vazirmatn font |
| Localization | `flutter_localizations`, `fa` locale |
| Calendar | Jalali (Shamsi) date support |

### Layering

```
lib/
├── core/                       # Cross-cutting concerns
│   ├── api_service.dart        # HTTP client with token management
│   ├── jwt_decoder.dart        # Client-side JWT decode (UX only)
│   ├── permissions.dart        # Permission constants + PermissionService
│   ├── app_colors.dart         # Design tokens (colors, spacing, typography)
│   ├── validators.dart         # Input validators
│   └── constants.dart          # App-wide constants
├── data/
│   ├── dio_client.dart         # Dio HTTP client with interceptors
│   └── repositories/           # Data access layer
│       ├── salon_repository.dart
│       ├── artist_repository.dart
│       ├── service_repository.dart
│       ├── appointment_repository.dart
│       └── notification_repository.dart
├── domain/                     # Business entities
│   ├── salon.dart
│   ├── artist.dart
│   ├── service.dart
│   ├── appointment.dart
│   └── notification.dart
├── presentation/
│   ├── pages/                  # Screen widgets
│   │   ├── splash_screen.dart
│   │   ├── otp_screen.dart
│   │   ├── salon_card.dart
│   │   ├── salon_detail_screen.dart
│   │   ├── admin/
│   │   │   └── admin_dashboard.dart
│   │   └── manager/
│   │       └── artist_management_screen.dart
│   ├── providers/              # Riverpod state notifiers
│   │   ├── salon_providers.dart
│   │   ├── artist_providers.dart
│   │   ├── service_providers.dart
│   │   ├── appointment_providers.dart
│   │   └── notification_providers.dart
│   └── widgets/                # Reusable components
│       ├── dashboard_widgets.dart
│       └── permission_gate.dart
├── l10n/                       # Localization (fa, en)
├── main.dart                   # App entry point, theme, routing
└── widgets/
    └── error_boundary.dart     # Global error handler widget
```

### Role-Based Navigation

**File:** `smart_salon_app/lib/presentation/pages/admin/admin_dashboard.dart`

The app shows different navigation tabs based on the user's role (decoded from JWT):

| Role | Available Tabs |
|------|---------------|
| Admin | Dashboard, Salons, Users, Settings, Reports |
| Manager | Dashboard, Artists, Services, Appointments, Inventory, Finance |
| Artist | Dashboard, My Schedule, My Performance, My Profile |
| Client | Home, Search, My Bookings, Loyalty, Profile |

### Permission Gating

**File:** `smart_salon_app/lib/core/permissions.dart`

```dart
class PermissionService {
  final Set<String> _permissions;
  const PermissionService(Set<String> permissions) : _permissions = permissions;

  bool can(String permission) => _permissions.contains(permission);
  bool canAll(List<String> permissions) => permissions.every(_permissions.contains);
  bool canAny(List<String> permissions) => permissions.any(_permissions.contains);
}
```

**§R8:** Client-side gating is **UX only** — the server enforces every rule independently. Assume the client is hostile.

### API Service

**File:** `smart_salon_app/lib/core/api_service.dart`

```dart
class ApiService {
  static const _storage = FlutterSecureStorage();
  
  // Token management: save, read, clear
  // HTTP methods: get, post, put, delete
  // Automatic Bearer token injection
  // Response handling with UTF-8 decoding
  // 401 → throws 'session_expired'
}
```

---

## 12. Build Roadmap & Current Status

The access control implementation follows a 10-phase plan defined in `SalonOS_Access_Control_Design.md`:

| Phase | Description | Status |
|-------|-------------|--------|
| **1** | Permission foundation (constants + role map) | ✅ Complete |
| **2** | Authorization plumbing (requirement, handler, policy provider, attribute, DI) | 🔄 In Progress |
| **3** | Current user + claims (ICurrentUser, JWT permissions, short-lived token) | 🔄 In Progress |
| **4** | Tenant scoping (ITenantContext, query filters, write stamping, middleware) | 🔄 In Progress |
| **5** | Ownership checks (OwnsAppointment handler) | ⬜ Not Started |
| **6** | Lock down controllers ([HasPermission] on all actions) | ⬜ Not Started |
| **7** | Cross-tenant admin + Receptionist role | ⬜ Not Started |
| **8** | Database RLS (predicate function, policy, session context interceptor) | ⬜ Not Started |
| **9** | Tests (tenant isolation, IDOR, privilege escalation, contract visibility) | ⬜ Not Started |
| **10** | Flutter client gating (hide UI by permission) | ⬜ Not Started |

### Key TODOs

1. **Event handlers**: `BookingCompletedHandler` and `BookingCancelledHandler` are stubs — need inventory, notification, and community stat logic
2. **Refresh token system**: 30-min access token requires refresh token infrastructure
3. **RLS migration scripts**: SQL scripts for predicate functions and security policies on all tenant tables
4. **Cross-tenant isolation tests**: xUnit test suite in `tests/SalonOS.Tenancy.Tests/`
5. **Flutter endpoint migration**: Point `ApiService` from SmartSalon (:5015) to SalonOS via Nginx proxy (`/api/`)

---

## 13. Key Engineering Decisions & Trade-offs

### 1. Modular Monolith vs Microservices

**Decision:** Modular monolith — one deployable host, bounded contexts as class library projects.

**Rationale:** Operational simplicity for a small team. Module boundaries are enforced architecturally (interface-only references) so extraction to microservices is possible later.

### 2. Permission-in-JWT vs Server-Side Lookup

**Decision:** Embed permissions as individual `permission` claims in the JWT.

**Rationale:** Avoids a database hit on every request for authorization. Trade-off: permissions are stale until token refresh. Mitigated by 30-minute token lifetime (down from 30 days).

### 3. EF Global Query Filters vs Manual Scoping

**Decision:** Global query filters on `ITenantContext.TenantId`.

**Rationale:** Eliminates the class of bugs where a developer forgets to add `.Where(x => x.TenantId == ...)`. The filter is applied automatically to every query.

### 4. SQL Server RLS as Defense-in-Depth Floor

**Decision:** RLS is Layer 3, not the primary isolation mechanism.

**Rationale:** EF query filters (Layer 2) are the primary defense. RLS catches any query that bypasses the application layer (e.g., direct SQL, reporting tools, future microservices).

### 5. Dual-Backend During Transition

**Decision:** SmartSalon (:5015) and SalonOS (:5016) run simultaneously.

**Rationale:** Allows incremental migration. Flutter client can be switched by changing the API base URL (routed through Nginx).

### 6. Client-Side Permission Gating

**Decision:** Flutter reads permissions from JWT to hide UI elements.

**Rationale:** UX improvement — users don't see buttons that will just 403. Explicitly NOT a security boundary (§R8).

### 7. Single Sanctioned Cross-Tenant Path

**Decision:** Only `PlatformAdminService` may call `IgnoreQueryFilters()`.

**Rationale:** Prevents accidental cross-tenant data leaks. All other code is physically incapable of querying across tenants.

---

## 14. File Index

### Backend — SalonOS

| File | Purpose |
|------|---------|
| `src/SalonOS.Api/Program.cs` | Host: middleware pipeline, DI wiring |
| `src/SalonOS.Shared/Authorization/Permissions.cs` | 77 permission constants |
| `src/SalonOS.Shared/Authorization/RolePermissions.cs` | Role → permissions map |
| `src/SalonOS.Shared/Authorization/PermissionRequirement.cs` | Authorization requirement |
| `src/SalonOS.Shared/Authorization/PermissionHandler.cs` | Checks JWT permission claims |
| `src/SalonOS.Shared/Authorization/PermissionPolicyProvider.cs` | Maps "perm:xxx" policy names |
| `src/SalonOS.Shared/Authorization/HasPermissionAttribute.cs` | `[HasPermission("...")]` attribute |
| `src/SalonOS.Shared/Identity/ICurrentUser.cs` | Current user interface |
| `src/SalonOS.Shared/MultiTenancy/ITenantContext.cs` | Tenant context interface |
| `src/SalonOS.Shared/MultiTenancy/TenantEntity.cs` | Base entity with TenantId |
| `src/SalonOS.Api/Authorization/OwnsAppointment.cs` | Ownership check handler |
| `src/SalonOS.Api/Middleware/TenantResolutionMiddleware.cs` | Tenant validation middleware |
| `src/SalonOS.Infrastructure/Identity/CurrentUser.cs` | Reads user from HTTP context claims |
| `src/SalonOS.Infrastructure/MultiTenancy/TenantContextFromClaims.cs` | Tenant context from ICurrentUser |
| `src/SalonOS.Infrastructure/Interceptors/TenantSessionContextInterceptor.cs` | RLS session context interceptor |
| `src/SalonOS.Infrastructure/Admin/PlatformAdminService.cs` | Cross-tenant admin (only IgnoreQueryFilters) |
| `src/SalonOS.Infrastructure/Jobs/OutboxDispatcherJob.cs` | Outbox message dispatcher |
| `src/SalonOS.Infrastructure/Jobs/ReminderJob.cs` | Appointment reminder job |
| `src/SalonOS.Infrastructure/EventHandlers/BookingCompletedHandler.cs` | Booking completed handler (stub) |
| `src/SalonOS.Infrastructure/EventHandlers/BookingCancelledHandler.cs` | Booking cancelled handler (stub) |
| `src/Modules/Identity/Infrastructure/AuthService.cs` | Auth service + JWT builder |
| `src/Modules/Identity/Domain/ApplicationUser.cs` | User entity |
| `src/Modules/Identity/Domain/Tenant.cs` | Tenant entity |
| `src/Modules/Identity/Domain/Membership.cs` | User-tenant membership |
| `src/Modules/Booking/Domain/Booking.cs` | Booking entity |
| `src/Modules/Booking/Domain/Events/BookingCompleted.cs` | Domain event |
| `src/Modules/Booking/Domain/Events/BookingCancelled.cs` | Domain event |
| `src/Modules/Catalog/Domain/CatalogService.cs` | Service entity |

### Backend — SmartSalon (Legacy)

| File | Purpose |
|------|---------|
| `SmartSalon/SmartSalon/Controllers/` | 8 controllers (Salons, Artists, Services, Appointments, etc.) |
| `SmartSalon/SmartSalon/Services/` | Business logic layer |
| `SmartSalon/SmartSalon/Validators/` | FluentValidation validators |
| `SmartSalon/SmartSalon/DTOs/` | Request/response DTOs |
| `SmartSalon/SmartSalon/Models/` | EF Core entities |
| `SmartSalon/SmartSalon/Data/ApplicationDbContext.cs` | Database context |

### Flutter Client

| File | Purpose |
|------|---------|
| `smart_salon_app/lib/main.dart` | App entry, theme, RTL, localization |
| `smart_salon_app/lib/core/api_service.dart` | HTTP client with token management |
| `smart_salon_app/lib/core/jwt_decoder.dart` | Client-side JWT decode |
| `smart_salon_app/lib/core/permissions.dart` | Permission constants + PermissionService |
| `smart_salon_app/lib/core/app_colors.dart` | Design tokens |
| `smart_salon_app/lib/core/validators.dart` | Input validators |
| `smart_salon_app/lib/data/dio_client.dart` | Dio HTTP client |
| `smart_salon_app/lib/data/repositories/` | Data access layer |
| `smart_salon_app/lib/domain/` | Business entities |
| `smart_salon_app/lib/presentation/pages/` | Screen widgets |
| `smart_salon_app/lib/presentation/providers/` | Riverpod state notifiers |
| `smart_salon_app/lib/presentation/widgets/` | Reusable components |
| `smart_salon_app/lib/widgets/error_boundary.dart` | Global error handler |

### Infrastructure

| File | Purpose |
|------|---------|
| `docker-compose.yml` | 4-service orchestration |
| `nginx.conf` | Reverse proxy: `/api/` → SalonOS, else Flutter SPA |
| `Dockerfile.SalonOS` | SalonOS API Docker image |
| `Dockerfile.FlutterWeb` | Flutter Web + Nginx Docker image |
| `.env.example` | Environment variable template |

### Documentation

| File | Purpose |
|------|---------|
| `SalonOS-Architecture-DotNet-Flutter.md` | Architecture design document |
| `SalonOS_Access_Control_Design.md` | Access control design + build tasks |
| `SalonOS-Implementation-Steps.txt` | Implementation checklist |
| `PROJECT_SUMMARY.txt` | Project overview |

---

*End of Technical Report*