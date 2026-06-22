# Agent Tasks — Artist build (A1–A5)

Implements the optimized Artist spec (`Artist-spec.md`). Same low-risk pattern as the manager
features: new entities inherit `TenantEntity` → auto tenant-scoped by `AppDbContext`.

> Rule: do ONLY what each step says. If a "Find" isn't found exactly, STOP and report.
> "My own" data is resolved from the JWT claim `User.FindFirst("artist_id")`.

## Already working (no task needed)
- View daily schedule / assigned appointments → `GET /api/artist-schedule/my` ✅
- Daily/month customer count → `GET /api/artist-schedule/my/stats` ✅
- Receive instructions (notices) → `GET /api/salon/notices` (Artist has `SalonView`) ✅
- Complete / cancel own appointment → `AppointmentComplete` / `AppointmentCancelOwn` ✅

## Order

| # | File | Feature | Flag |
|---|------|---------|------|
| A1 | `A1-leave-and-contract.md` | Artist can request leave + view own contracts (+ all new perms) | 🟡 |
| A2 | `A2-client-notes.md` | Customer notes / suggestions / product tips (`ClientNote`) | 🟡 |
| A3 | `A3-staff-requests.md` | Report issues + equipment requests (`StaffRequest`) | 🟡 |
| A4 | `A4-checkin-and-reschedule.md` | Check-in field + reschedule REQUEST | 🟡 |
| A5 | `A5-product-usage.md` | Record products consumed (`ProductUsage`) | 🟡 |
| AZ | `AZ-rls.md` | Put the 4 new artist tables under RLS (do LAST) | 🟡 |

Do A1 first (it adds ALL the new Artist permissions in one go). After everything: `dotnet build SalonOS.slnx`.

## Stays with Claude (design)
- Item 12 "multiple services performed per visit" → part of the booking state-machine work.
- Decrementing real inventory from `ProductUsage` (cross-module) → Claude.
- Contract-based leave auto-approval (fixed→pending, rental→auto) → refinement after A1.
