# Agent Tasks — Batch 2: fix public pages after RLS

Why: turning RLS on made the anonymous public endpoints return empty data, because they
query RLS-protected tables (`Bookings`, `ArtistSchedules`) with no resolved tenant.

This batch fixes it with the **minimal** approach:
- **Track A** — store the salon's rating as denormalized columns on the global `Tenants`
  row, so public pages never query `Bookings`. (Fixes the rating regression.)
- **Track B** — resolve the tenant from the public `slug` for the anonymous slots endpoint,
  so RLS scopes reads to that one salon. (Fixes anonymous availability.)

Hand ONE file at a time, in order. Flags: 🟢 safe · 🟡 do it, then you review.

> Rule for the agent: make ONLY the change in the file. If "Find" is not found exactly, STOP and report.

## Order

| # | File | What | Flag |
|---|------|------|------|
| A01 | `A01-tenant-rating-columns.md` | Add `RatingSum` + `RatingCount` to `Tenant` | 🟡 |
| A02 | `A02-rating-migration.md` | EF migration for the new columns | 🟡 |
| A03 | `A03-rating-updater-interface.md` | `ISalonRatingUpdater` interface (Shared) | 🟢 |
| A04 | `A04-rating-updater-impl.md` | Implement it in Identity | 🟡 |
| A05 | `A05-rating-updater-di.md` | Register it in `Program.cs` | 🟡 |
| A06 | `A06-bookingservice-inject.md` | Inject updater into `BookingService` | 🟡 |
| A07 | `A07-bookingservice-call.md` | Call updater in `RateAsync` | 🟡 |
| A08 | `A08-getsalons-read-columns.md` | `GetSalons` reads columns, not `Bookings` | 🟢 |
| A09 | `A09-getsalonbyslug-read-columns.md` | `GetSalonBySlug` reads columns | 🟢 |
| B01 | `B01-itenantcontext-setter.md` | Add `SetPublicTenant` to interface | 🟡 |
| B02 | `B02-tenantcontext-impl.md` | Implement `SetPublicTenant` | 🟡 |
| B03 | `B03-public-slots-endpoint.md` | New `GET /api/salons/{slug}/slots` | 🟡 |

After this batch: build (`dotnet build SalonOS.slnx`). For a quick check, an anonymous
`GET /api/salons/{slug}/slots?artistId=...&date=...` should return slots (not empty).

## Note for the Flutter side (not an agent task)
Point the public artist/salon page to the NEW `GET /api/salons/{slug}/slots` instead of the
old `GET /api/booking/slots` (the old one has no tenant and stays empty for anonymous).
