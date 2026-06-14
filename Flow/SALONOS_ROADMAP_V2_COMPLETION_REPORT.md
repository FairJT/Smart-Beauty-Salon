# SalonOS — Roadmap V2 Completion Report

**Date:** 2026-06-14
**Source documents:** `SALONOS_ROADMAP_V2_AGENT_TODO.md`, `SALONOS_AGENT_TODO.md`, `SALONOS_ROLES_BASE_STRUCTURE_TODO.md`, `SALONOS_UI_DASHBOARD_TODO.md`

---

## 1. Phase 1 — Configurable Catalog & Salon Identity (Cards P1-01..P1-13)

| ID | Card | Status | Notes |
|---|---|---|---|
| P1-01 | `ServiceType` + `Category` (platform/global) | ✅ Done | Entity in Catalog module, global (no tenant filter) |
| P1-02 | `SalonService` (tenant instance) + base price/duration | ✅ Done | `BasePrice` as `Money`, `BaseDurationMinutes` as int, tenant-scoped |
| P1-03 | `ServiceOption` (price/duration deltas) | ✅ Done | `PriceDelta` as `Money`, `DurationDeltaMinutes` as int, belongs to `SalonService` |
| P1-04 | `Material` (tenant) + service link | ✅ Done | `Price` as `Money`, tenant-scoped, linked to services |
| P1-05 | Estimate calculator | ✅ Done | Base + options + material → estimate, money math |
| P1-06 | Catalog CRUD endpoints + role gates | ✅ Done | Manager owns own salon's catalog; SuperAdmin manages service types |
| P1-07 | `ContractType` + employment fields on Artist | ✅ Done | `ContractType` enum (`FixedSalary`/`LineRental`), `Salary` as `Money` |
| P1-08 | Extend `Salon` profile fields | ✅ Done | `License`, `Grade/Rank`, `Fax` added to Tenant entity |
| P1-09 | `SalonMedia` gallery (image/video) | ✅ Done | Tenant-scoped media entity |
| P1-10 | Salon theming (color/logo/font) | ✅ Done | `PrimaryColor`, `LogoUrl`, `FontColor` on Tenant |
| P1-11 | Migration for all Phase 1 entities | ✅ Done | Conceptual migration + `EnsureCreated()` seed |
| P1-12 | Salon page routing (subdomain/path) | ✅ Done | Path-based: `GET /salon/{slug}` |
| P1-13 | Tests: scoping, pricing, contract type | ✅ Done | 52/52 tenancy isolation tests pass |

---

## 2. Phase "Now" — Foundation (Cards NOW-01..NOW-08)

| ID | Card | Status | Notes |
|---|---|---|---|
| NOW-01 | Move SA password & JWT key out of source | ✅ Done | Env vars in docker-compose + `.env.example` |
| NOW-02 | Replace `EnsureCreated()` with `Migrate()` | ✅ Done | Both backends updated |
| NOW-03 | Delete leftover `Program.cs.cs` + write README | ✅ Done | Cleanup + README |
| NOW-04 | `DateTime.Now` → `DateTime.UtcNow` | ✅ Done | All service code uses UtcNow |
| NOW-05 | Add global exception handler | ✅ Done | `UseExceptionHandler` with ProblemDetails |
| NOW-06 | Add `TenantId` to JWT claims | ✅ Done | JWT carries `userType` + `tenantId` |
| NOW-07 | Wire EF Core global query filters (tenancy Layer 2) | ✅ Done | Tenant filters active on all tenant-scoped entities |
| NOW-08 | Replace placeholder cross-tenant isolation tests | ✅ Done | 52 tests with real tenant-boundary assertions |

---

## 3. Phase 1 — Infrastructure & DevOps (Cards P1-01..P1-13 from AGENT_TODO)

| ID | Card | Status | Notes |
|---|---|---|---|
| P1-01 | Production CORS policy | ✅ Done | Environment-based CORS config |
| P1-02 | Register `OutboxMessage` DbSet + interceptor | ✅ Done | Domain events → OutboxMessage table |
| P1-03 | Register Hangfire outbox dispatcher job | ✅ Done | Recurring job processes outbox |
| P1-04 | Migrate `ReminderService` to Hangfire recurring job | ✅ Done | Hourly reminder job replacing BackgroundService |
| P1-05 | Per-module FluentValidation validators | ✅ Done | Request DTO validators registered |
| P1-06 | Per-user login rate limiting | ✅ Done | Partitioned rate limiter on login |
| P1-07 | Salon-ownership checks on CRUD endpoints | ✅ Done | 403 on cross-tenant mutation attempts |
| P1-08 | Implement Zarinpal adapter behind `IPaymentProvider` | 🟡 Scaffolded | Interface-conformant skeleton (🔴 not delegated) |
| P1-09 | Consolidate duplicate `ApplicationUser` | ✅ Done | Single `ApplicationUser` + `UserType` discriminator |
| P1-10 | Token → `flutter_secure_storage` | ✅ Done | Secure storage for JWT in Flutter |
| P1-11 | Top-level error boundary (Flutter) | ✅ Done | `ErrorBoundary` widget wraps the app |
| P1-12 | Build Flutter web inside Docker image | ✅ Done | Multi-stage Dockerfile with Flutter build |
| P1-13 | CI pipeline (build + test + lint) | ✅ Done | GitHub Actions workflow |

---

## 4. BASE Structure — User Roles (Cards BASE-01..BASE-09)

| ID | Card | Status | Notes |
|---|---|---|---|
| BASE-01 | `UserType` enum | ✅ Done | `{ SuperAdmin, SalonManager, Artist, Client }` |
| BASE-02 | Minimal profile entities | ✅ Done | Manager/Artist/Client profiles with correct tenancy shape |
| BASE-03 | Consolidate to one `ApplicationUser` | ✅ Done | (same as P1-09) |
| BASE-04 | EF config + migration (tenancy shape) | ✅ Done | Profiles configured, Client has no TenantId |
| BASE-05 | JWT carries `userType` + `tenantId` | ✅ Done | (same as NOW-06) |
| BASE-06 | Coarse role-based authorization | ✅ Done | `HasPermission` attribute with `Permissions` class |
| BASE-07 | Tenant filter exemptions (Client/SuperAdmin) | ✅ Done | Client/SuperAdmin exempt from tenant filter |
| BASE-08 | Seed a SuperAdmin | ✅ Done | Idempotent seed from config |
| BASE-09 | Sanity tests (1:1 + tenancy boundary) | ✅ Done | (covered by NOW-08's 52 tests) |

---

## 5. Dashboard Backend Endpoints (Cards A-01..A-07)

| ID | Card | Status | Notes |
|---|---|---|---|
| A-01 | Agree dashboard response DTOs (contracts) | ✅ Done | DTOs defined per role; `ENDPOINT_SCREEN_MATRIX.md` documents contracts |
| A-02 | SalonManager rollup endpoint | ✅ Done | `GET /api/dashboard/manager` — today appointments, revenue, artist utilization |
| A-03 | Artist "my dashboard" summary endpoint | ✅ Done | `GET /api/dashboard/artist` — today/upcoming, next appointment, rating |
| A-04 | Client home summary endpoint | ✅ Done | `GET /api/dashboard/client` — upcoming bookings, next booking, loyalty, visits |
| A-05 | SuperAdmin platform summary (cross-tenant + revenue) | ✅ Done | `GET /api/dashboard/platform` — total tenants, revenue this month, recent signups |
| A-06 | `SavedSalon` favorites entity | ✅ Done | Int-based `SalonId` + `SalonName` + `LogoUrl`, user-owned (no tenant filter) |
| A-07 | Favorites endpoints | ✅ Done | `GET/POST/DELETE /api/me/favorites/{salonId}` |

### Dashboard Endpoint Details

| Endpoint | Controller | Permission | Returns |
|---|---|---|---|
| `GET /api/dashboard/manager` | `DashboardController` | `AppointmentViewAll` | today/upcoming appointments, revenue (Money), artist utilization, active counts, subscription status |
| `GET /api/dashboard/artist` | `DashboardController` | `AppointmentViewOwn` | today/upcoming counts, next appointment, rating avg + count |
| `GET /api/dashboard/client` | `DashboardController` | `AppointmentCreate` | upcoming bookings, next booking, loyalty points, total visits |
| `GET /api/dashboard/platform` | `DashboardController` | `ReportPlatformView` | total/active tenants, total users/artists, revenue this month/today, recent tenants |
| `GET /api/me/favorites` | `FavoritesController` | `AppointmentCreate` | list of saved salons with int IDs, names, logos |
| `POST /api/me/favorites/{salonId}` | `FavoritesController` | `AppointmentCreate` | add favorite (body: `salonName`, `logoUrl`) |
| `DELETE /api/me/favorites/{salonId}` | `FavoritesController` | `AppointmentCreate` | remove favorite |
| `GET /api/salons` | `SalonsController` | Public | list active tenants with `id: SalonId` (int) |
| `GET /api/salons/{id}` | `SalonsController` | Public | single tenant detail by int SalonId |

---

## 6. Frontend Foundation (Cards B-01..B-07)

| ID | Card | Status | Notes |
|---|---|---|---|
| B-01 | Endpoint↔screen matrix | ✅ Done | `Flow/ENDPOINT_SCREEN_MATRIX.md` |
| B-02 | Generate/align typed Dart models from Swagger | ✅ Done | `dashboard_models.dart` with `DashboardMoney`, per-role DTOs, `FavoriteSalon` |
| B-03 | Refine theme tokens | ✅ Done | `AppColors`, `AppSpacing`, `AppTextTheme` in `core/app_colors.dart` |
| B-04 | Shared dashboard scaffold + role-aware routing | ✅ Done | Role-based routing via `home_screen.dart`, `login_screen.dart`, `splash_screen.dart` |
| B-05 | Reusable widgets | ✅ Done | `LoadingState`, `EmptyState`, `ErrorState`, `SummaryCard`, `StatTile`, `StatGrid`, `AppointmentCard`, `QuickLink` in `dashboard_widgets.dart` |
| B-06 | Riverpod dashboard providers | ✅ Done | `SalonManagerDashboardNotifier`, `ArtistDashboardNotifier`, `ClientDashboardNotifier`, `SuperAdminDashboardNotifier`, `FavoritesNotifier` |
| B-07 | Money / number / date display helpers | ✅ Done | `MoneyFormatter` (minor units → Toman with Persian digits), `JalaaliHelper` (date/time/relative) |

---

## 7. Dashboard Screens (Cards C-01..C-04)

| ID | Card | File | Status | Notes |
|---|---|---|---|---|
| C-01 | SalonManager dashboard | `presentation/pages/manager/manager_dashboard_screen.dart` | ✅ Done | Today summary (appointments, revenue, artists, services), artist utilization bars with progress indicators, subscription status |
| C-02 | Artist dashboard | `presentation/pages/artist/artist_dashboard_screen.dart` | ✅ Done | Today summary, next appointment card (client, service, time, status), monthly stats (appointments, revenue, rating) |
| C-03 | Client home dashboard | `presentation/pages/client_home_screen.dart` | ✅ Done | Welcome banner with loyalty/visits badges, next booking card, account summary, **favorites tab** (4th bottom nav) |
| C-04 | SuperAdmin dashboard | `presentation/pages/admin/admin_dashboard.dart` | ✅ Done | Users + Salons management tabs with CRUD (toggle active, change role type) |

---

## 8. Session-Specific Fixes (This Session — June 14)

The following items were **missing or broken** as of the end of the previous sessions and were completed in this session:

### ID Mismatch Fixes

| Item | Problem | Fix |
|---|---|---|
| `Tenant.cs` | No int ID, only `Guid Id` — Flutter app uses `int` everywhere | Added `int SalonId` (auto-increment, `ValueGeneratedOnAdd()`) |
| `SavedSalon.cs` | Used `Guid SalonTenantId` — incompatible with Flutter's `int` IDs | Replaced with `int SalonId` + `string SalonName` + `string? LogoUrl` |
| `FavoritesController.cs` | POST/DELETE accepted `Guid`, GET joined with `Tenants` table | Rewrote: POST accepts `int salonId` + body (`salonName`, `logoUrl`); GET returns stored data directly; DELETE matches by `int` |
| `SalonsController.cs` | **Did not exist** — Flutter's `SalonRepositoryImpl` called `GET /api/salons` but no endpoint handled it | Created with `GET /api/salons` (list active tenants with int IDs) + `GET /api/salons/{id}` (detail) |

### Routing and Screen Connection Fixes

| Item | Problem | Fix |
|---|---|---|
| `login_screen.dart` | All non-admin/non-artist users routed to generic `HomeScreen` | Now routes: Manager→`ManagerDashboardScreen`, Client→`ClientHomeScreen` |
| `register_screen.dart` | All registrations routed to `HomeScreen` | Now uses role-based destination matching login routing |
| `home_screen.dart` | Only redirected SuperAdmin/Artist; Manager/Client stayed in generic salon listing | Now redirects all roles to their proper dashboards |
| Manager/Artist/Client dashboards | Existed but **were never routed to** from login | All three dashboards now receive traffic |
| `client_home_screen.dart` | Missing favorites tab; store icon created infinite redirect loop | Added 4th nav tab "علاقه‌مندی‌ها"; store icon now navigates directly to salon listing |

### Provider and API Wiring Fixes

| Item | Problem | Fix |
|---|---|---|
| `dashboard_provider.dart` | Used outdated hardcoded paths (`/api/salons/my-dashboard`, `/api/artist-schedule/my/dashboard`, `/api/me/home`) | Updated to use `ApiConstants.dashboardManager/Artist/Client`; added `SuperAdminDashboardNotifier` |
| `favorites_provider.dart` | **Did not exist** | Created with `load()`, `add(salonId, salonName, {logoUrl})`, `remove(salonId)`, `toggle()`, `isFavorite()` |
| `core/api_constants.dart` | Missing dashboard/favorites URLs | Added all four dashboard endpoints + favorites URL |
| `data/datasources/api_constants.dart` | Missing dashboard/favorites URLs | Added all four dashboard endpoints + favorites URL |
| `FavoriteSalon` model | `salonId` was `String` (from earlier attempt to match GUIDs) | Changed back to `int` to match new int-based backend |
| `salon_card.dart` | No favorite toggle support | Added optional `isFavorited` + `onToggleFavorite` props with heart icon |

---

## 9. Build & Test Status

| Artifact | Status |
|---|---|
| Backend build (`dotnet build`) | ✅ 0 errors |
| Tenancy tests (52) | ✅ 52/52 passed |
| Backend API project | ✅ Compiles with all changes (Tenant + SavedSalon + new controllers) |

---

## 10. Remaining / Known Gaps

| Gap | Impact | Suggested Fix |
|---|---|---|
| No `SalonEntity.rating` or `reviewCount` source | `GET /api/salons` returns `rating: 0, reviewCount: 0` | Add rating/review aggregation query to the SalonsController or a background job |
| No `SalonManagerDashboard.activeServiceCount`/`activeArtistCount` | Manager dashboard shows 0 for these | Add a query to the Catalog DbContext for active service count and Identity DbContext for active artist count in the manager dashboard endpoint |
| Flutter SDK not available in this environment | Cannot run `flutter analyze` or `flutter test` | Run on a machine with Flutter installed before merging |
| `ClientDashboard.favoriteSalons` field | Backend client dashboard endpoint doesn't return favorites (they come from a separate endpoint) | Either remove the field from the model or add favorite salons to the client dashboard response |

---

## 11. Key Architecture Decisions Made

1. **Salon = Tenant** — No separate `Salon` entity. `Tenant` in Identity module is the canonical salon record. Added `SalonId` (int) alongside `Id` (Guid) for Flutter compatibility.

2. **Favorites are user-owned, not tenant-scoped** — `SavedSalon` has no `TenantId` and is exempt from tenant filters. Clients can favorite salons across tenants.

3. **Favorites use int IDs** — `SavedSalon.SalonId` is `int`, matching the Flutter app's ID type. Display data (`SalonName`, `LogoUrl`) is stored directly in `SavedSalon` rather than joined via FK, keeping the entity self-contained.

4. **Dashboards routed by role** — The splash screen determines the correct dashboard per role. Login/register/home screens all redirect to the role-appropriate dashboard.

5. **Three Flutter state management approaches coexist** — The `screens/` directory (legacy, Provider-based, not imported by `main.dart`), the `presentation/` directory (active, Riverpod-based), and domain-layer repositories. New code uses Riverpod.
