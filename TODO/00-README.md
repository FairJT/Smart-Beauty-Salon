# Agent Tasks — Client build (C1–C4)

Implements the agent-safe parts of the Client capabilities, consistent with Manager/Artist.
Same pattern: new entities inherit `TenantEntity` → auto tenant-scoped by `AppDbContext`.
Client id = `User.FindFirst(ClaimTypes.NameIdentifier)`; target salon = `_tenant.TenantId` (resolved
from the salon context, same as bookings).

> Rule: do ONLY what each step says. If a "Find" isn't found exactly, STOP and report.

## Already working (no task)
- Register/login, edit profile, book, choose artist, choose date/time, cancel own, rate,
  view past services + previous artist → all exist (Client already has the permissions).
- View services / prices / durations → catalog read (C1 adds `CatalogView`).

## Order

| # | File | Feature (items) | Flag |
|---|------|-----------------|------|
| C1 | `C1-permissions.md` | Client can browse catalog + file feedback (perms) | 🟡 |
| C2 | `C2-feedback.md` | Suggestion / complaint (`ClientFeedback`) — item 16 | 🟡 |
| C3 | `C3-offers.md` | View discounts + validate a code — items 10, 17(safe) | 🟡 |
| C4 | `C4-invoice.md` | View invoice (derived from booking) — item 12 | 🟢 |
| CZ | `CZ-rls.md` | Put `ClientFeedbacks` under RLS (do LAST) | 🟡 |

Do C1 first. After everything: `dotnet build SalonOS.slnx`.

## Stays with Claude (money / infra / design — NOT agent)
- Online payment full flow (item 11) + invoice tied to a real payment (12 full).
- Applying a discount code to the price + incrementing `UsedCount` (item 17 full) — money-critical.
- SMS special offers (item 18) — needs an `INotificationProvider` abstraction.
- "Change appointment" (item 7) = cancel + re-book for now (no new entity).
