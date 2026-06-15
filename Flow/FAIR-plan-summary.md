## Goal
- Complete all tasks from `Flow/FAIR-flutter-fixes-revised.md`: remove `int` from all API surfaces, centralize Flutter IDs on `String`, migrate `SavedSalon` to slug, standardize sub-resource URLs, centralize JSON mapping.

## Constraints & Preferences
- `Flow/FAIR-flutter-fixes-revised.md` supersedes `Flow/FAIR-remaining-flutter-todos.md`.
- **STRAT-01**: Single identifier policy — slug for public, Guid for entity PKs, **no `int` anywhere in API request/response**.
- `int SalonId` never for auth/scoping (FAIR-07 ruling).
- 🟡 (agent drafts, human reviews): all BE/FE/QAI tasks — implement now, human reviews later.
- 🔴 (do not delegate): STRAT-01 (already decided), BE-04 (clustered PK).
- Flutter SDK unavailable; Flutter changes are source-only.

## Progress
### Done
- FAIR-01: EnsureCreated audit — zero references across all `.cs` files.
- FAIR-02: Generated 4 real EF migrations with proper `Money` owned-column mapping.
- FAIR-04: Estimate calculator — 11 tests written and passing.
- FAIR-05: Auth enforcement tests — 22 tests covering `PermissionHandler`, all role-permission matrices, cross-role boundaries, anonymous/no-role rejection.
- FAIR-06: Slug strategy — `SalonsController.GetSalonBySlug`, `FavoritesController` includes slug, Flutter `SalonDetailScreen`/`BookingScreen`/`GuestBookingScreen` use `String slug`, home/ client-home navigate with slug, `FavoriteSalon` model has slug field.
- FAIR-07: `int SalonId` audit — 3 references, none used for scoping/auth.
- FAIR-08: Auth gate audit — report produced, 6 missing/wrong gates fixed.
- FAIR-09: Permission remap — `ClientSelf` added, `FavoritesController`/`DashboardController` gated.
- FAIR-10: No legacy `screens/` directory.
- FAIR-11: Favorites staleness — auto-refreshes `SalonName`/`LogoUrl` on read.
- FAIR-12: `JobSeekerProfile` entity exists with proper fields.
- FAIR-13: Salon ratings computed in `SalonsController`.
- FAIR-14: Active counts computed in `DashboardController`.
- FAIR-15: `ClientDashboard.favoriteSalons` refreshed with staleness + real rating.
- FAIR-16: Zarinpal provider — `CreatePaymentAsync`/`VerifyPaymentAsync`/`VerifyWebhook` implementing v4 REST API with proper request/response shapes and `ZarinpalResponse` DTOs.
- **Backend–Flutter alignment audit**: Fixed all route/response mismatches.
- **BE-01/BE-03 (Backend int removal + SavedSalon slug migration)**: Completed.
- **FE-01 (Centralize ID types)**: Created `lib/types.dart` with `typedef SalonId/ArtistId/ServiceId/AppointmentId/NotificationId = String`.
- **FE-02 (Sweep Flutter to String IDs)**: All entities, models, repos, providers, and screens converted. No `int` entity ID remains in Flutter.
- **FE-03 (Standardize sub-resource URLs)**: Repos use `api/artists/salon/{slug}` and `api/services/salon/{slug}` (backend updated to accept slug).
- **FE-04 (Centralize JSON mapping)**: Removed all fallback JSON parsing (`startsAt`/`startTime`, `estimatedPrice`/`estimatedPriceAmount`). All repos use consistent field names matching backend contract.
- **QA-01**: Build succeeds with 0 errors, 0 warnings. All 229 tests pass (83 + 146).

### Pending
- FAIR-03: Flutter compile — no Flutter SDK.
- BE-04: Verify clustered-PK strategy (🔴 human decision).
- QA-02: Re-audit — no `int`/surrogate used for scoping in any layer.

## Key Decisions
- **Identifier policy (STRAT-01/FAIR-06)**: Slug for public, Guid for entity PKs. **No `int` in any API surface.** `FavoriteSalon.salonId` → `slug`.
- **`SavedSalon` migration**: `SalonId` (int) → `Slug` (string). EF migration `20260615131758_MigrateSavedSalonToSlug`.
- **Flutter ID alias**: Single `types.dart` module — one-line type changes instead of 15-file sweeps.
- **Sub-resource URLs**: `api/artists/salon/{slug}` and `api/services/salon/{slug}` (backend controllers accept slug).
- `Flow/FAIR-flutter-fixes-revised.md` is the authoritative task list.

## Relevant Files
- `Flow/FAIR-flutter-fixes-revised.md`: master task list.
- `smart_salon_app/lib/types.dart`: typedef aliases for all ID types.
- `smart_salon_app/lib/domain/entities/*.dart`: all entities now use `String id`.
- `smart_salon_app/lib/domain/repositories/*.dart`: interfaces accept `String` IDs.
- `smart_salon_app/lib/data/models/*.dart`: `fromJson` parses IDs as String.
- `smart_salon_app/lib/data/repositories/*.dart`: slug-based URLs, no fallback parsing.
- `smart_salon_app/lib/presentation/providers/*.dart`: method signatures use `String` IDs.
- `smart_salon_app/lib/presentation/pages/*.dart`: route params use `String`.
- `src/Modules/Identity/Domain/SavedSalon.cs`: `Slug` replaces `SalonId`.
- `src/Modules/Identity/Infrastructure/Migrations/20260615131758_MigrateSavedSalonToSlug.cs`.
- Backend controllers: `salonId` removed from all responses; routes accept slug.
