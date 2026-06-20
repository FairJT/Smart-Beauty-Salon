# Agent Tasks — Batch 3: fixes from the full repo review

Small, ordered, one-change-per-file tasks for the local agent (deepseek-coder).
Hand ONE file at a time, in order. Flags: 🟢 safe · 🟡 do it, then you review.

> Rule for the agent: make ONLY the change in the file. If "Find" isn't found exactly, STOP and report.
> Independent of the `TODO/update/` batch (the public-pages fix) — you can do either order.
> The two big design items (booking deposit/hold state machine) are NOT here — those stay with Claude.

## Order

| # | File | What | Flag |
|---|------|------|------|
| A1 | `A1-unique-booking-slot.md` | Prevent double-booking (filtered unique index) + migration | 🟡 |
| A2 | `A2-rls-add-identity-tables.md` | Put `ArtistProfiles`/`Memberships`/`SalonManagerProfiles` under RLS | 🟡 |
| A3 | `A3-remove-duplicate-migration.md` | Remove the empty duplicate JobSeeker migration | 🟡 |
| B1 | `B1-consolidate-role-routing.md` | Remove the inconsistent role gate from `home_screen` | 🟡 |
| B2 | `B2-fix-api-constants.md` | Fix wrong API paths (catalog/inventory/marketplace) | 🟢 |
| B3 | `B3-delete-dead-screens.md` | Delete unused legacy `screens/` files | 🟢 |
| C1 | `C1-money-formatter-comment.md` | Fix misleading money comment | 🟢 |
| C2 | `C2-persian-compact-suffixes.md` | Persian magnitude words instead of K/M/B | 🟢 |
| C3 | `C3-api-base-url-note.md` | Document prod API base URL (nginx) | 🟢 |

After this batch: build (`dotnet build SalonOS.slnx`) and `flutter analyze` in `smart_salon_app`.

## Not in this batch (kept on Claude)
- The money UNIT contract (is `Money.Amount` always Rials?) needs a real decision + validation — design task.
- Booking deposit + hold + state machine — design task, will be its own batch.
