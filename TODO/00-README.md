# Agent Tasks — SalonManager full build (M2–M8)

These implement the rest of the SalonManager spec. Each file = one feature with small numbered
steps. Do files in order; inside a file do steps in order. Flags: 🟢 safe · 🟡 review after.

> Rule: do ONLY what each step says. If a "Find" isn't found exactly, STOP and report.
> Apply Batch M1 (`agent-tasks-4`) FIRST. Then these.

## The repeating pattern (why this is low-risk)

`AppDbContext` (in `src/SalonOS.Infrastructure/AppDbContext.cs`) automatically gives EVERY class
that inherits `TenantEntity`:
- tenant scoping (`TenantId == current tenant`), RLS session context, soft-delete filter, and
- **auto-stamps `TenantId` on save** — so controllers never set it.

So each feature is just: **(1)** create an entity inheriting `TenantEntity`, **(2)** add a `DbSet`
to `AppDbContext`, **(3)** migration, **(4)** a controller injecting `AppDbContext`. No new context,
no manual tenant filtering.

- Entity files go in `src/SalonOS.Infrastructure/SalonMgmt/` with `namespace SalonOS.Infrastructure;`
- Controllers go in `src/SalonOS.Api/Controllers/` with `namespace SalonOS.Api.Controllers;`
- Migration command (always the same context):
  ```powershell
  dotnet ef migrations add <Name> --project src\SalonOS.Infrastructure --startup-project src\SalonOS.Api --context AppDbContext
  ```

## Order

| # | File | Feature | Flag |
|---|------|---------|------|
| M2 | `M2-amenities-and-notices.md` | Salon amenities + notice board | 🟡 |
| M3 | `M3-hours-and-closures.md` | Working hours + salon closures | 🟡 |
| M4 | `M4-staff-service-contracts.md` | Per-service staff contracts + discount | 🟡 |
| M5 | `M5-finance-and-support.md` | Financial-transaction ledger + support staff | 🟡 |
| M6 | `M6-discounts.md` | Discounts / coupon codes | 🟡 |
| M7 | `M7-hiring.md` | Job postings + applications | 🟡 |
| M8 | `M8-manager-views.md` | Manager review view + customer list | 🟢 |
| MZ | `MZ-rls-and-permissions.md` | Put new tables under RLS + map hiring perms | 🟡 |

Do MZ LAST (after the entities exist). After everything: `dotnet build SalonOS.slnx`.

## Not here (stays with Claude — design)
- Money unit contract (Rial enforcement) · Booking deposit/hold state machine · the cross-tenant
  jobseeker browse side of hiring · the other entities (SuperAdmin / Artist / Client).
