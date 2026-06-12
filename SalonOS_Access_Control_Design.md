# SalonOS — Access Control: Design + Build Tasks (single file)

This ONE file contains everything needed to add hierarchical access control to
SalonOS. It has two parts:

- **PART 1 — REFERENCE:** the design and the exact code. This is the source of truth.
- **PART 2 — BUILD TASKS:** small steps to do in order. Each task points back to a
  PART 1 section (e.g. "see §R6.1") for the code to copy.

> AGENT: read "HOW TO USE THIS FILE", "GLOBAL RULES", and "ROADMAP" first. Then
> go to PART 2 and do one task at a time, in order. Scroll up to PART 1 when a
> task tells you to.

---

## HOW TO USE THIS FILE (agent, read first)

GOAL in one paragraph: Add hierarchical access control. Five roles —
PlatformOwner, SalonManager, Receptionist, Artist, Client. Every protected action
must pass THREE checks together: (1) PERMISSION the user holds, (2) TENANT the row
belongs to, (3) OWNERSHIP for "own"-scoped actions. Authorize on PERMISSIONS, never
on role names. Never trust a tenant id sent by the client.

WHERE TO WORK — the new modular backend:
```
src/SalonOS.Api/
src/SalonOS.Shared/
src/SalonOS.Infrastructure/
src/Modules/Identity, Booking, Catalog, Inventory, Marketplace
tests/
```
FIRST ACTION before any code: open the repo and confirm these folders exist. If a
path in a task does not exist, search for the closest match, use it, and write one
line saying which path you used. Never put code in a random place.

---

## GLOBAL RULES (re-read if you get confused)

R1. Do ONE task per step. After finishing, stop and report files changed + a
    1-line summary. Then continue.
R2. Authorize on permission strings (e.g. "appointment.cancel.all"), NEVER on role
    names inside handlers or controllers.
R3. Tenant id ALWAYS comes from the logged-in user's token/context. NEVER from a
    request body, query string, or route value. Code that reads tenantId from
    input is a bug.
R4. Writes set TenantId from the tenant context, not from the incoming DTO.
R5. Exactly ONE file may cross tenants: PlatformAdminService (Task 7.1). No other
    file may call IgnoreQueryFilters().
R6. Do not delete or rewrite working business logic. You are ADDING a security
    layer around it.
R7. After each PHASE, run `dotnet build` if you can and fix errors first. If you
    cannot build, re-read your new files for typos / missing usings / wrong
    namespace before continuing.
R8. If a task is unclear or a file is missing, STOP and ask the human. Do not guess
    and do not invent endpoints.

---

## ROADMAP (keep your place here)

```
[x] PHASE 1  Permission foundation            (Tasks 1.1–1.2)
[ ] PHASE 2  Authorization plumbing           (Tasks 2.1–2.5)
[ ] PHASE 3  Current user + claims            (Tasks 3.1–3.3)
[ ] PHASE 4  Tenant scoping                   (Tasks 4.1–4.5)
[ ] PHASE 5  Ownership checks                 (Tasks 5.1–5.2)
[ ] PHASE 6  Lock down the controllers        (Tasks 6.1–6.6)
[ ] PHASE 7  Cross-tenant admin + Receptionist(Tasks 7.1–7.2)
[ ] PHASE 8  Database Row-Level Security       (Tasks 8.1–8.2)
[ ] PHASE 9  Tests                            (Tasks 9.1–9.4)
[ ] PHASE 10 Flutter client gating (last)      (Task 10.1)
```
Do the phases in this exact order. Each builds on the one before it.

================================================================================
# PART 1 — REFERENCE (design + code)
================================================================================

## §R1. Core principle: access is the intersection of three checks

```
        ACCESS GRANTED  ==  PERMISSION  ∧  TENANT  ∧  OWNERSHIP
                            (what you    (whose     (is this
                             can do)      salon)     row yours)
```

- PERMISSION — does the user hold the permission for this action? (role → permissions)
- TENANT — does the row belong to the salon the user acts in? A SalonManager manages
  their OWN salon only.
- OWNERSHIP — for "own"-scoped actions, does the row actually belong to this user?

Role alone never authorizes anything. Loyalty tiers (Bronze/Silver/Gold/Diamond)
are NOT access roles — they are a Client segmentation attribute and stay out of the
authorization layer entirely.

## §R2. Role hierarchy

```
PLATFORM SCOPE  (crosses tenants — the only sanctioned cross-tenant path)
└── PlatformOwner     (your "SuperAdmin") — runs the SaaS, all salons, marketplace, billing

TENANT SCOPE  (one salon = one tenant)
├── SalonManager      (salon owner/admin) — full control of *their* salon
├── Receptionist [NEW](front desk)        — booking + desk payments, no admin
├── Artist            (stylist/staff)      — own schedule, own performance, own payout
└── Client            (consumer)           — own bookings, own loyalty, public browsing
```
Roles are NOT nested. Model each as a distinct permission set, not inheritance.

## §R3. Permission catalog

Format: `resource.action[.scope]`, scope = `own` (only my rows) or `all` (any row in my tenant).

| Resource | Permissions |
|---|---|
| Salon | salon.view, salon.edit, salon.settings.manage |
| Staff / Artist | staff.view, staff.create, staff.edit, staff.delete, staff.contract.manage, staff.performance.view |
| Catalog / Service | catalog.view, catalog.create, catalog.edit, catalog.delete, catalog.package.manage |
| Appointment | appointment.view.all, appointment.view.own, appointment.create, appointment.confirm, appointment.complete, appointment.cancel.all, appointment.cancel.own, appointment.rate |
| Inventory | inventory.view, inventory.adjust, inventory.manage |
| Finance | finance.revenue.view, finance.deposit.take, finance.payout.view.own, finance.payout.manage, finance.period.close |
| Reports | report.salon.view, report.staff.view.own, report.platform.view |
| Loyalty | loyalty.config.manage, loyalty.view.own |
| Notification | notification.send, notification.view.own |
| Marketplace | marketplace.browse, marketplace.license.purchase, marketplace.template.manage |
| Platform / Tenant | tenant.manage, tenant.billing.manage, platform.config.manage, platform.audit.view |

## §R4. Permission matrix

`✓` = granted. `own` = self-scoped only. Blank = denied. The four tenant roles are
always bounded to their OWN tenant.

| Permission | PlatformOwner | SalonManager | Receptionist | Artist | Client |
|---|:--:|:--:|:--:|:--:|:--:|
| salon.view | ✓ | ✓ | ✓ | ✓ | public |
| salon.edit / settings.manage | ✓ | ✓ | | | |
| staff.view | ✓ | ✓ | ✓ | own | |
| staff.create / edit / delete | ✓ | ✓ | | | |
| staff.contract.manage | ✓ | ✓ | | | |
| staff.performance.view | ✓ | all | | own | |
| catalog.view | ✓ | ✓ | ✓ | ✓ | public |
| catalog.create / edit / delete | ✓ | ✓ | | | |
| catalog.package.manage | ✓ | ✓ | | | |
| appointment.view | ✓ | all | all | own | own |
| appointment.create | ✓ | ✓ | ✓ | | ✓ |
| appointment.confirm | ✓ | ✓ | ✓ | own | |
| appointment.complete | ✓ | ✓ | ✓ | own | |
| appointment.cancel | ✓ | all | all | own | own |
| appointment.rate | | | | | ✓ |
| inventory.view | ✓ | ✓ | ✓ | | |
| inventory.adjust / manage | ✓ | ✓ | | | |
| finance.revenue.view | ✓ | ✓ | | | |
| finance.deposit.take | ✓ | ✓ | ✓ | | |
| finance.payout.view.own | ✓ | all | | own* | |
| finance.payout.manage / period.close | ✓ | ✓ | | | |
| report.salon.view | ✓ | ✓ | | | |
| report.staff.view.own | ✓ | all | | own | |
| loyalty.config.manage | ✓ | ✓ | | | |
| loyalty.view.own | | | | | ✓ |
| notification.send | ✓ | ✓ | ✓ | | |
| notification.view.own | ✓ | ✓ | ✓ | ✓ | ✓ |
| marketplace.browse | ✓ | ✓ | | | |
| marketplace.license.purchase | ✓ | ✓ | | | |
| marketplace.template.manage | ✓ | | | | |
| tenant.manage / billing.manage | ✓ | | | | |
| platform.config / audit.view | ✓ | | | | |

`*` Artist payout visibility is contract-dependent — see §R5.

## §R5. Artist contract types change financial visibility

| Contract | Persian | Money the Artist may see |
|---|---|---|
| Salaried | حقوق ثابت | Only own ratings & completed count. NO revenue figures. |
| Chair rental | اجاره خط | Own service revenue and own deposits. |
| Room rental | اجاره اتاق | Same as chair rental, plus their room's utilization. |

Implement as a runtime check inside `finance.payout.view.own`: grant the permission
to all Artists, then return revenue only when `artist.ContractType != Salaried`.
One permission, contract-aware response shaping — not separate roles.

## §R6. Enforcement code (ASP.NET Core 9)

### §R6.1 Permission-based policies (copy these as-is)

```csharp
public sealed class PermissionRequirement(string permission) : IAuthorizationRequirement
{
    public string Permission { get; } = permission;
}

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

public sealed class PermissionPolicyProvider(IOptions<AuthorizationOptions> options)
    : IAuthorizationPolicyProvider
{
    private readonly DefaultAuthorizationPolicyProvider _fallback = new(options);

    public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        const string prefix = "perm:";
        if (policyName.StartsWith(prefix, StringComparison.Ordinal))
        {
            var policy = new AuthorizationPolicyBuilder()
                .AddRequirements(new PermissionRequirement(policyName[prefix.Length..]))
                .Build();
            return Task.FromResult<AuthorizationPolicy?>(policy);
        }
        return _fallback.GetPolicyAsync(policyName);
    }

    public Task<AuthorizationPolicy> GetDefaultPolicyAsync() => _fallback.GetDefaultPolicyAsync();
    public Task<AuthorizationPolicy?> GetFallbackPolicyAsync() => _fallback.GetFallbackPolicyAsync();
}

public sealed class HasPermissionAttribute(string permission)
    : AuthorizeAttribute($"perm:{permission}");
```

Usage on a controller action:
```csharp
[HttpPost("{id}/cancel")]
[HasPermission("appointment.cancel.all")]
public Task<IActionResult> Cancel(Guid id) { /* ... */ }
```

### §R6.2 Role → permissions map + the 30-day JWT trap

The current token lasts 30 days. If you bake permissions into it, a role change or
a fired employee keeps access for a month. Fix: short-lived access token (30 min) +
refresh; rebuild permissions on refresh. Keep the mapping in ONE file:

```csharp
public static readonly Dictionary<string, string[]> RolePermissions = new()
{
    ["SalonManager"] = new[] { "salon.edit", "salon.settings.manage", "staff.create",
        "staff.edit", "staff.delete", "staff.contract.manage", "staff.performance.view",
        "catalog.create", "catalog.edit", "catalog.delete", "catalog.package.manage",
        "appointment.view.all", "appointment.confirm", "appointment.complete",
        "appointment.cancel.all", "inventory.view", "inventory.adjust", "inventory.manage",
        "finance.revenue.view", "finance.deposit.take", "finance.payout.manage",
        "finance.period.close", "report.salon.view", "loyalty.config.manage",
        "notification.send", "marketplace.browse", "marketplace.license.purchase",
        "salon.view", "catalog.view", "staff.view", "notification.view.own" },
    ["Receptionist"] = new[] { "appointment.view.all", "appointment.create",
        "appointment.confirm", "appointment.complete", "appointment.cancel.all",
        "finance.deposit.take", "catalog.view", "staff.view", "salon.view",
        "inventory.view", "notification.send", "notification.view.own" },
    ["Artist"] = new[] { "appointment.view.own", "appointment.confirm",
        "appointment.complete", "appointment.cancel.own", "report.staff.view.own",
        "staff.performance.view", "finance.payout.view.own", "catalog.view",
        "salon.view", "notification.view.own" },
    ["Client"] = new[] { "appointment.view.own", "appointment.create",
        "appointment.cancel.own", "appointment.rate", "loyalty.view.own",
        "notification.view.own" },
    // PlatformOwner is handled by the §R6.4 bypass, NOT a permission list.
};
```

### §R6.3 Tenant scope — EF Core global query filter (layer 2)

```csharp
public interface ITenantContext { Guid TenantId { get; } bool IsPlatformOwner { get; } }

protected override void OnModelCreating(ModelBuilder b)
{
    foreach (var et in b.Model.GetEntityTypes()
                 .Where(t => typeof(TenantEntity).IsAssignableFrom(t.ClrType)))
    {
        b.Entity(et.ClrType).HasIndex(nameof(TenantEntity.TenantId));
        var p = Expression.Parameter(et.ClrType, "e");
        var body = Expression.Equal(
            Expression.Property(p, nameof(TenantEntity.TenantId)),
            Expression.Property(Expression.Constant(_tenant), nameof(ITenantContext.TenantId)));
        b.Entity(et.ClrType).HasQueryFilter(Expression.Lambda(body, p));
    }
}
```
Writes set TenantId from context:
```csharp
var appt = new Appointment { /* ...dto... */ TenantId = _tenant.TenantId };
```

### §R6.4 The single sanctioned cross-tenant path

```csharp
// PlatformAdminService.cs — the ONLY place cross-tenant reads are allowed
public Task<List<Salon>> AllSalonsAsync() =>
    _db.Salons.IgnoreQueryFilters().ToListAsync();   // guarded by [HasPermission("tenant.manage")]
```

### §R6.5 Ownership for "own" actions (stops IDOR)

```csharp
public sealed class OwnsAppointment : IAuthorizationRequirement;

public sealed class OwnsAppointmentHandler(ICurrentUser user)
    : AuthorizationHandler<OwnsAppointment, Appointment>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext ctx, OwnsAppointment req, Appointment appt)
    {
        var owns = user.Role == "Artist" ? appt.ArtistId == user.ArtistId
                 : user.Role == "Client" ? appt.ClientId == user.UserId
                 : false;
        if (owns) ctx.Succeed(req);
        return Task.CompletedTask;
    }
}
// In the controller:
// await _authz.AuthorizeAsync(User, appointment, new OwnsAppointment());
```

### §R6.6 Database — SQL Server Row-Level Security (the floor)

```sql
CREATE FUNCTION Security.fn_tenant(@TenantId uniqueidentifier)
RETURNS TABLE WITH SCHEMABINDING
AS RETURN
  SELECT 1 AS ok
  WHERE @TenantId = CAST(SESSION_CONTEXT(N'TenantId') AS uniqueidentifier)
     OR CAST(SESSION_CONTEXT(N'IsPlatformOwner') AS bit) = 1;
GO
CREATE SECURITY POLICY Security.TenantFilter
  ADD FILTER PREDICATE Security.fn_tenant(TenantId) ON dbo.Appointments,
  ADD BLOCK  PREDICATE Security.fn_tenant(TenantId) ON dbo.Appointments AFTER INSERT
  WITH (STATE = ON);
```
Set the session context once per request:
```csharp
await db.Database.ExecuteSqlAsync(
    $"EXEC sp_set_session_context N'TenantId', {_tenant.TenantId};");
```

## §R7. Per-role pages & dashboards

Two surfaces: web admin (PlatformOwner, SalonManager, Receptionist) and the Flutter
app (Client + an Artist mode). Each role lands on a different dashboard.

PlatformOwner — web admin only:
- Dashboard: active tenants, MRR, total bookings, sign-up funnel, system health.
- Tenants (onboard/suspend/impersonate-audited), Marketplace templates, Billing,
  Platform settings (providers, feature flags), Audit log.

SalonManager — web admin (their salon only):
- Dashboard: today's appointments, occupancy %, revenue today, deposits, no-shows, low-stock.
- Calendar (all artists), Staff + contracts + performance, Catalog + packages,
  Inventory, Finance (revenue/deposits/payouts/period close/reports), Loyalty config,
  Notifications, Salon settings (hours, theme, VIP).

Receptionist — web admin (subset):
- Dashboard: today's schedule + check-in, walk-in/guest booking, client lookup.
- Calendar (book/confirm/complete/cancel), take 30% deposit, Clients (search/create).
- HIDDEN: contracts, finance reports/revenue, inventory management, settings, loyalty config.

Artist — Flutter Artist mode (and/or web):
- Dashboard: next appointment, today's count, my rating, my earnings (only if chair/room rental).
- My schedule (own only), My performance (own), My profile (bio/photo/services/hours).
- HIDDEN: other artists' schedules, salon finance, inventory, settings.

Client — Flutter app:
- Home (search/nearby/VIP), Salon detail, Booking (service→artist→slot→30% deposit via Zarinpal),
  Guest booking, My appointments (cancel under 2-hr rule), Reviews, Loyalty (points/tier),
  Profile, Notifications.

## §R8. Client-side gating is UX, not security

Store the permission list in the app's auth state and hide buttons/tabs the user
lacks. This only avoids dead buttons — the server still enforces every rule. Assume
the client is hostile.

## §R9. Mandatory tests

1. Tenant isolation: tenant A and B with data; as A, read+mutate B's rows by id → both fail.
2. Cross-user (IDOR): as Artist X, complete/cancel Artist Y's appointment → 403.
3. Privilege escalation: as Receptionist, call finance.period.close + salon.settings.manage → 403.
4. Contract visibility: salaried Artist own payout → no revenue; chair-rental → revenue present.

If a test fails, fix the code, not the test.

================================================================================
# PART 2 — BUILD TASKS (do in order, one at a time)
================================================================================

## PHASE 1 — PERMISSION FOUNDATION

### TASK 1.1 — Permission constants
FILE: src/SalonOS.Shared/Authorization/Permissions.cs
DO: static class `Permissions` with one `public const string` per permission in §R3.
    Example: `public const string AppointmentCancelOwn = "appointment.cancel.own";`
DONE WHEN: compiles; one const per permission in §R3.
[x] complete

### TASK 1.2 — Role → permissions map
FILE: src/SalonOS.Shared/Authorization/RolePermissions.cs
DO: static class with `Dictionary<string,string[]> Map` using the bundles in §R6.2,
    referencing the §1.1 constants. 4 keys: SalonManager, Receptionist, Artist, Client.
    PlatformOwner is NOT in the map.
DONE WHEN: Map has the 4 roles with the §R6.2 lists.
[x] complete
>>> STOP. Report Phase 1 done.

## PHASE 2 — AUTHORIZATION PLUMBING

### TASK 2.1 — PermissionRequirement
FILE: src/SalonOS.Api/Authorization/PermissionRequirement.cs — copy from §R6.1.
[ ] complete
### TASK 2.2 — PermissionHandler
FILE: src/SalonOS.Api/Authorization/PermissionHandler.cs — copy from §R6.1.
[ ] complete
### TASK 2.3 — PermissionPolicyProvider
FILE: src/SalonOS.Api/Authorization/PermissionPolicyProvider.cs — copy from §R6.1.
[ ] complete
### TASK 2.4 — HasPermission attribute
FILE: src/SalonOS.Api/Authorization/HasPermissionAttribute.cs — copy from §R6.1.
[ ] complete
### TASK 2.5 — Register in DI
FILE: src/SalonOS.Api/Program.cs
DO: add
        builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
        builder.Services.AddScoped<IAuthorizationHandler, PermissionHandler>();
    Ensure app.UseAuthentication() then app.UseAuthorization() are both present, in that order.
DONE WHEN: app builds and starts.
[ ] complete
>>> STOP. Report Phase 2 done.

## PHASE 3 — CURRENT USER + CLAIMS

### TASK 3.1 — ICurrentUser
FILES: src/SalonOS.Shared/Identity/ICurrentUser.cs (interface),
       src/SalonOS.Infrastructure/Identity/CurrentUser.cs (impl reading claims via
       IHttpContextAccessor). Expose: UserId, ArtistId?, Role, TenantId, IsPlatformOwner.
DO: register AddHttpContextAccessor() and AddScoped<ICurrentUser, CurrentUser>().
DONE WHEN: ICurrentUser injects and returns UserId.
[ ] complete

### TASK 3.2 — Put permissions into the login token
FILE: the JWT creation code in Module Identity (search for where the token is built).
DO: add one claim type "permission" per permission from RolePermissions.Map for the
    user's role; add "tenant_id"; add "artist_id" if Artist; for PlatformOwner add
    "is_platform_owner"="true" instead of a permission list.
DONE WHEN: a decoded login token has permission claims + tenant_id.
[ ] complete

### TASK 3.3 — Short-lived token + refresh (security fix; see §R6.2)
DO: access token lifetime = 30 min; add a revocable refresh token; rebuild permission
    claims on refresh. If a refresh system is too large for one step, STOP and ask the
    human — do NOT fake it.
DONE WHEN: access token expires in 30 min; refresh re-issues a new one.
[ ] complete
>>> STOP. Report Phase 3 done.

## PHASE 4 — TENANT SCOPING (most important — go slow)

### TASK 4.1 — ITenantContext
FILES: src/SalonOS.Shared/MultiTenancy/ITenantContext.cs,
       src/SalonOS.Infrastructure/MultiTenancy/TenantContext.cs (fills from ICurrentUser).
DO: register Scoped. (Interface shape is in §R6.3.)
DONE WHEN: ITenantContext returns the logged-in tenant id.
[ ] complete

### TASK 4.2 — Confirm TenantId on entities
DO: confirm TenantEntity has non-null Guid TenantId and that Salon, Artist, Appointment,
    Service, Inventory, Notification inherit it. If one does not, make it inherit and LIST
    the needed migration for the human (do not run it).
DONE WHEN: every tenant-owned entity has TenantId.
[ ] complete

### TASK 4.3 — EF global query filter
FILE: the DbContext OnModelCreating in src/SalonOS.Infrastructure — copy §R6.3. Inject ITenantContext.
DONE WHEN: an appointments query returns only the current tenant's rows with no extra Where in services.
[ ] complete

### TASK 4.4 — Writes set TenantId from context
DO: at every `new <TenantEntity>` / `.Add(...)`, set `.TenantId = _tenant.TenantId`. Remove any
    code copying TenantId from a DTO/request (R3, R4).
DONE WHEN: no create path reads tenant id from client input.
[ ] complete

### TASK 4.5 — Tenant resolution middleware (safety net)
FILE: src/SalonOS.Api/Middleware/TenantResolutionMiddleware.cs — register AFTER UseAuthentication,
      BEFORE UseAuthorization. Populate TenantContext from the user's claims; reject 401 if a
      protected endpoint has no tenant and the user is not PlatformOwner.
DONE WHEN: requests without a valid tenant claim are rejected.
[ ] complete
>>> STOP. Report Phase 4 done — double-check it prevents leaks.

## PHASE 5 — OWNERSHIP CHECKS

### TASK 5.1 — OwnsAppointment requirement + handler
FILE: src/SalonOS.Api/Authorization/OwnsAppointment.cs — copy §R6.5. Register handler.
[ ] complete
### TASK 5.2 — Apply ownership to "own" endpoints
DO: in the Appointments controller/service, for cancel-own / complete-own / confirm-own, after
    loading the appointment call `await _authz.AuthorizeAsync(User, appointment, new OwnsAppointment());`
    and return 403 on failure.
DONE WHEN: an Artist cannot cancel/complete another Artist's appointment by id.
[ ] complete
>>> STOP. Report Phase 5 done.

## PHASE 6 — LOCK DOWN THE CONTROLLERS
Add [HasPermission("...")] to each action using the §R4 matrix. One controller per task; build after each.

### TASK 6.1 — AuthController: [AllowAnonymous] on Register/Login; protect the rest.
[ ] complete
### TASK 6.2 — SalonsController: salon.view / salon.edit / salon.settings.manage.
[ ] complete
### TASK 6.3 — ArtistsController: staff.* ; performance = staff.performance.view.
[ ] complete
### TASK 6.4 — ServicesController: catalog.* .
[ ] complete
### TASK 6.5 — AppointmentsController: appointment.* ; "own" ones also use Task 5.2.
[ ] complete
### TASK 6.6 — NotificationsController: notification.send / notification.view.own.
[ ] complete
>>> STOP. Report Phase 6 done.

## PHASE 7 — CROSS-TENANT ADMIN + RECEPTIONIST

### TASK 7.1 — PlatformAdminService (the ONLY cross-tenant file)
FILE: src/SalonOS.Infrastructure/Admin/PlatformAdminService.cs — copy §R6.4. Guard its controller
      actions with [HasPermission("tenant.manage")]. No other file calls IgnoreQueryFilters().
DONE WHEN: PlatformOwner lists all salons; nobody else can.
[ ] complete

### TASK 7.2 — Add the Receptionist role
DO: ensure "Receptionist" exists where roles are defined/seeded; its bundle is already in
    RolePermissions.Map. Confirm it has booking + finance.deposit.take and does NOT have
    finance.revenue.view, salon.settings.manage, staff.contract.manage, inventory.manage.
DONE WHEN: a Receptionist can book + take a deposit but gets 403 on settings/revenue/contracts/inventory.
[ ] complete
>>> STOP. Report Phase 7 done.

## PHASE 8 — DATABASE ROW-LEVEL SECURITY

### TASK 8.1 — RLS predicate + policy
FILE: a new SQL migration/script — copy §R6.6 for Appointments, Salons, Artists, Services,
      Inventory, Notifications. FILTER + BLOCK predicates, STATE = ON.
DONE WHEN: a raw SQL select with a different session tenant returns no other-tenant rows.
[ ] complete

### TASK 8.2 — Set session context per request
FILE: the DbContext / a connection interceptor — run sp_set_session_context with the request
      tenant (and IsPlatformOwner) after opening the connection (§R6.6).
DONE WHEN: RLS uses the live request tenant automatically.
[ ] complete
>>> STOP. Report Phase 8 done.

## PHASE 9 — TESTS (nothing is "done" without these passing; see §R9)

### TASK 9.1 — Tenant isolation test (§R9 #1).
[ ] complete
### TASK 9.2 — Cross-user / IDOR test (§R9 #2).
[ ] complete
### TASK 9.3 — Privilege escalation test (§R9 #3).
[ ] complete
### TASK 9.4 — Contract visibility test (§R9 #4).
[ ] complete
>>> STOP. Report Phase 9 done. If a test fails, fix the CODE, not the test.

## PHASE 10 — FLUTTER CLIENT GATING (cosmetic, last)

### TASK 10.1 — Hide screens by permission (§R7, §R8)
FILE: the Flutter app auth state (Riverpod).
DO: store the user's permission list after login; hide tabs/buttons the user lacks. This is ONLY
    to avoid dead buttons — never move a real check to the client.
DONE WHEN: each role sees only its own navigation, while the API still blocks anything the UI missed.
[ ] complete

================================================================================
# FINAL CHECK — do not declare success until ALL are true
================================================================================
```
[ ] Every protected controller action has a [HasPermission(...)] attribute.
[ ] No code reads tenant id from request body / query / route.
[ ] IgnoreQueryFilters() appears in exactly ONE file (PlatformAdminService).
[ ] All four tests in Phase 9 pass.
[ ] `dotnet build` succeeds with no errors.
[ ] You wrote a short summary of every file you created or changed.
```
When all boxes are ticked, stop and hand back the summary.
