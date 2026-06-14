# SalonOS — Dashboards & UI Agent-Ready TODO

**For:** a free / local AI coding agent (e.g. Continue.dev + Ollama / deepseek-coder)
**Source:** `SALONOS_DESIGN_AND_IMPLEMENTATION_PLAN.md` + the dashboard decisions (Flutter client, all four roles in parallel, refine the existing Persian/RTL look)
**Generated:** 2026-06-13
**Prerequisite:** the **Now** phase of `SALONOS_AGENT_TODO.md` is done — especially NOW-07 (global query filters wired) and the `Money` type adopted. Tenant-scoped dashboard queries are only safe once filters are live.

This phase has two tracks of work: a small amount of **backend** (the dashboard summary endpoints and one new favorites entity — the data exists, the rollups don't) and the bulk of the work in **Flutter**. Cards are sized for a local agent; the sensitive backend rollups (cross-tenant, money) stay off the local model.

---

## How the agent must operate

The operating rules from `SALONOS_AGENT_TODO.md` carry over (one card at a time, read the relevant SKILL.md first, stay in scope, build+test after each change, no thrashing, no fake-passing tests, small commits). Additional rules for this phase:

1. **Do not change an API contract unilaterally.** If a screen needs a field an endpoint doesn't return, stop and report — the contract is agreed in card B-01/A-01, not improvised per screen.
2. **Format money only at display.** Amounts arrive as integer minor units + currency. Never do math on them in the UI; format with the shared helper (B-07). Per the payments skill.
3. **Reuse the shared widgets (B-05). Do not reinvent a card/stat tile per screen.**
4. **Respect RTL and Jalali.** Every new screen must render correctly right-to-left with Persian digits and `shamsi_date`.
5. **Every screen handles three states:** loading, empty, and error — not just the happy path.

## Delegation legend

| Flag | Meaning |
|---|---|
| 🟢 | Agent-safe. Mechanical UI / boilerplate. |
| 🟡 | Agent drafts; human reviews before merge (touches scoping, money display, or API wiring). |
| 🔴 | Do **not** delegate. Cross-tenant or money-aggregation logic. Claude/human authors; agent may scaffold structure only if the card says so. |

---

## Master index

| ID | Task | Track | Flag | Effort | Depends on |
|---|---|---|---|---|---|
| A-01 | Agree dashboard response DTOs (contracts) | Backend | 🟡 | S | NOW-07 |
| A-02 | SalonManager rollup endpoint (tenant-scoped) | Backend | 🟡 | M | A-01 |
| A-03 | Artist "my dashboard" summary endpoint | Backend | 🟡 | S | A-01 |
| A-04 | Client home summary endpoint | Backend | 🟡 | S | A-01 |
| A-05 | SuperAdmin platform summary (cross-tenant + revenue) | Backend | 🔴 | M | A-01 |
| A-06 | `SavedSalon` favorites entity + migration | Backend | 🔴 | S | — |
| A-07 | Favorites endpoints (add / remove / list) | Backend | 🟡 | S | A-06 |
| B-01 | Endpoint↔screen matrix | Frontend | 🟢 | S | A-01 |
| B-02 | Generate/align typed Dart models from Swagger | Frontend | 🟢 | M | A-01 |
| B-03 | Refine theme tokens (colors / spacing / Farsi type) | Frontend | 🟢 | M | — |
| B-04 | Shared dashboard scaffold + role-aware routing | Frontend | 🟡 | M | B-03 |
| B-05 | Reusable widgets (card, stat tile, list, chart, states) | Frontend | 🟢 | M | B-03 |
| B-06 | Riverpod dashboard providers (one per role) | Frontend | 🟡 | M | A-02..A-05, B-02 |
| B-07 | Money / number / date display helpers | Frontend | 🟡 | S | B-02 |
| C-01 | SalonManager dashboard screen (extend existing Admin) | Dashboard | 🟡 | M | B-04,B-05,B-06 |
| C-02 | Artist dashboard screen | Dashboard | 🟡 | M | B-04,B-05,B-06 |
| C-03 | Client home dashboard screen | Dashboard | 🟡 | M | B-04,B-05,B-06 |
| C-04 | SuperAdmin dashboard screen | Dashboard | 🟡 | M | B-04,B-05,B-06 |
| D-01 | Widget tests per dashboard | QA | 🟡 | M | C-01..C-04 |
| D-02 | Reconcile matrix — no orphan endpoints/screens | QA | 🟢 | S | C-01..C-04 |

**Sequencing:** A-01 first (it unblocks both tracks). Then Backend (A) and Frontend foundation (B) run in parallel against the agreed contracts. The four dashboards (C) start once B-04/05/06 exist, and all four run in parallel. QA (D) closes it out.

---

# Track A — Backend support

> Where these endpoints live: build each rollup in whichever backend currently serves that role's data (per the report, that is the **legacy** monolith — `AppointmentService`, `ArtistService.GetPerformanceReportAsync`, and `SuperAdminController` all live there). If a module has already been ported to the new backend by the time you build its endpoint, build it there instead, following the consolidation plan.

### A-01 — Agree dashboard response DTOs 🟡
**Effort:** S · **Depends:** NOW-07
**Files:** `DTOs/` (new dashboard DTOs), reflected in Swagger.
**Steps:** Define one response DTO per dashboard (SalonManager, Artist, Client, SuperAdmin) listing exactly the fields each screen needs (see C-01..C-04 for the field lists). Money fields are `{ amount: long, currency: string }`, never a single number.
**Done when:** four DTOs exist, appear in Swagger, and the field lists match what the C cards consume. This is the contract B-01/B-02 build against.

### A-02 — SalonManager rollup endpoint 🟡
**Effort:** M · **Depends:** A-01
**Files:** `Controllers/SalonsController.cs` (add `GET /api/salons/{id}/dashboard`), supporting service method.
**Steps:** Return today's appointment count, upcoming count, revenue for a date range (sum of completed `FinalPrice` as `Money`), and per-artist utilization. Query is tenant-scoped by the global filter from NOW-07 — never trust a `salonId` for cross-tenant reach; verify the caller owns the salon (see P1-07).
**Done when:** the endpoint returns correct numbers for the caller's own salon and 403 for a salon they don't own; a test covers both.
**Review focus:** money sums use `Money`; query stays inside the tenant filter.

### A-03 — Artist "my dashboard" summary 🟡
**Effort:** S · **Depends:** A-01
**Files:** `Controllers/ArtistScheduleController.cs` or `ArtistsController.cs` (`GET /api/artist/me/dashboard`).
**Steps:** Resolve the current artist from the token (not from a route param). Return today's + upcoming appointment counts, next appointment, and rating summary (`RatingAvg`, `RatingCount`).
**Done when:** returns data only for the authenticated artist; another artist's id cannot be requested.

### A-04 — Client home summary 🟡
**Effort:** S · **Depends:** A-01
**Files:** `Controllers/AppointmentsController.cs` or a new `MeController` (`GET /api/me/home`).
**Steps:** Return upcoming bookings count + next booking, loyalty points and total visits (from `ApplicationUser`), and unread notification count. Loyalty points are integers, not money.
**Done when:** returns the authenticated user's own summary; no user id accepted from the request.

### A-05 — SuperAdmin platform summary 🔴
**Effort:** M · **Depends:** A-01
**Why red:** this is the **one sanctioned cross-tenant read** in the system, and it sums revenue across tenants — both the cross-tenant bypass and the money aggregation are on the red line. Claude/human authors it in one clearly named place that deliberately bypasses the tenant filter. The agent may draft the *DTO shape* only and stop.
**Files (reference):** `Controllers/SuperAdminController.cs`.

### A-06 — `SavedSalon` favorites entity + migration 🔴
**Effort:** S · **Depends:** —
**Why red:** favorites are subtle. A client can favorite salons belonging to *different* tenants, so `SavedSalon` is **not** an ordinary tenant-owned row — if you stamp it with `TenantId` and let the global filter scope it, a client will never see favorites outside their "current" tenant. It hangs off the **user** and references salon identity globally. That tenant-owned-vs-global decision is exactly what the multi-tenancy skill says to get right up front. Claude/human designs the entity + query-filter exemption. The agent may, once designed, generate the migration from the agreed model and stop.

### A-07 — Favorites endpoints 🟡
**Effort:** S · **Depends:** A-06
**Files:** new `FavoritesController` (`POST /api/me/favorites/{salonId}`, `DELETE …`, `GET /api/me/favorites`).
**Steps:** Add/remove/list favorites for the authenticated user. Reads are by user id from the token, deliberately not tenant-filtered (per A-06's design).
**Done when:** a client can favorite salons across tenants and list them all back; another user's favorites are never returned.

---

# Track B — Frontend foundation

### B-01 — Endpoint↔screen matrix 🟢
**Effort:** S · **Depends:** A-01
**Files:** `docs/endpoint_screen_matrix.md` (new).
**Steps:** Table mapping every screen → the endpoints it calls, and every endpoint → its consuming screen(s). Flag any endpoint with no consumer and any screen needing a missing endpoint.
**Done when:** the matrix is complete and every dashboard screen's data needs trace to a real endpoint from Track A.

### B-02 — Typed Dart models from Swagger 🟢
**Effort:** M · **Depends:** A-01
**Files:** `lib/data/models/` (or `lib/models/`).
**Steps:** Generate/align Dart models for the dashboard DTOs and any drifted existing DTOs, from the Swagger/OpenAPI spec. Money is a `Money { int amount; String currency; }` model, never a `double`.
**Done when:** models compile and field-match Swagger; no `double` money fields.

### B-03 — Refine theme tokens 🟢
**Effort:** M · **Depends:** —
**Files:** `lib/core/` (colors/theme), `lib/main.dart` (theme wiring).
**Steps:** Centralise the existing identity into tokens: primary `#1B3A5C` navy + the gold admin accent, a spacing scale (4/8/12/16/24), corner radii, elevation, and a text theme tuned for Farsi (line height, font). Keep RTL global. No new identity — refine the current one.
**Done when:** screens read from the token set; the app looks the same but consistent; no hardcoded colors/spacings introduced.

### B-04 — Shared dashboard scaffold + role routing 🟡
**Effort:** M · **Depends:** B-03
**Files:** `lib/presentation/` shell widget, `lib/main.dart` routing.
**Steps:** One responsive dashboard shell (app bar, RTL nav drawer/bottom nav, content area). Route to the correct dashboard based on the authenticated `UserType`. A wrong-role route is blocked.
**Done when:** logging in as each role lands on that role's (initially empty) dashboard route; cross-role access is blocked.
**Review focus:** role is read from verified auth state, not from anything client-editable.

### B-05 — Reusable dashboard widgets 🟢
**Effort:** M · **Depends:** B-03
**Files:** `lib/presentation/widgets/`.
**Steps:** Build `SummaryCard`, `StatTile`, `SectionList`, a chart wrapper, and shared `Loading` / `Empty` / `Error` widgets — all RTL-aware and token-styled. These are the only building blocks the C cards use.
**Done when:** a demo/gallery screen renders each widget correctly in RTL.

### B-06 — Riverpod dashboard providers 🟡
**Effort:** M · **Depends:** A-02..A-05, B-02
**Files:** `lib/providers/` (one provider per role dashboard).
**Steps:** Each provider fetches its role's summary endpoint via the existing Dio client (token from `flutter_secure_storage`, per P1-10), exposing loading/data/error state. No business logic in the provider beyond fetch + expose.
**Done when:** each provider returns typed data from its endpoint and surfaces error/loading states.

### B-07 — Money / number / date display helpers 🟡
**Effort:** S · **Depends:** B-02
**Files:** `lib/core/format/` (new).
**Steps:** Helpers to format `Money` (integer minor units → Toman/currency string at display only), Persian digits, and Jalali dates via `shamsi_date`. No arithmetic on money here — display only.
**Done when:** a 150000-minor-unit IRR value renders as the correct Toman string with Persian digits; dates render Jalali.
**Review focus:** confirm no money math, only formatting.

---

# Track C — The four dashboards (build in parallel)

Each C card: compose the screen from B-05 widgets, bind to its B-06 provider, handle all three states, render RTL. Field lists below define what each screen shows (and therefore what A-01's DTOs must contain).

### C-01 — SalonManager dashboard 🟡
**Effort:** M · **Depends:** B-04, B-05, B-06
**Files:** extend existing **Admin Dashboard** screen (`lib/presentation/pages/`), don't start from scratch.
**Shows:** today's appointment count, upcoming count, revenue (range), per-artist utilization, quick links to catalog/staff, subscription status.
**Done when:** renders live data from A-02 for the manager's own salon, with loading/empty/error states.

### C-02 — Artist dashboard 🟡
**Effort:** M · **Depends:** B-04, B-05, B-06
**Files:** new screen in `lib/presentation/pages/`.
**Shows:** today's + upcoming appointments, next appointment card, rating summary, link to schedule.
**Done when:** renders live data from A-03 for the authenticated artist, all states handled.

### C-03 — Client home dashboard 🟡
**Effort:** M · **Depends:** B-04, B-05, B-06
**Files:** new/!extended home screen in `lib/presentation/pages/`.
**Shows:** next booking, upcoming count, loyalty points + visits, favorite salons (from A-07), unread notifications.
**Done when:** renders live data from A-04 (+ A-07 favorites), all states handled.

### C-04 — SuperAdmin dashboard 🟡
**Effort:** M · **Depends:** B-04, B-05, B-06
**Files:** new screen in `lib/presentation/pages/`.
**Shows:** total tenants/salons, active subscriptions, platform revenue, package management link. (Data is the cross-tenant A-05 endpoint — the screen just renders it.)
**Done when:** renders live data from A-05, all states handled.

---

# Track D — QA & matching

### D-01 — Widget tests per dashboard 🟡
**Effort:** M · **Depends:** C-01..C-04
**Files:** `test/` (currently empty).
**Steps:** For each dashboard, a widget test asserting it renders the loading, populated, and error states correctly (mock the provider). No constant assertions.
**Done when:** four passing widget tests; each fails if the wrong state renders.

### D-02 — Reconcile the matrix 🟢
**Effort:** S · **Depends:** C-01..C-04
**Files:** `docs/endpoint_screen_matrix.md`.
**Steps:** Verify against the running app: no endpoint lacks a consumer, no screen calls a missing endpoint, no DTO field is unused or absent. Record the result.
**Done when:** the matrix has zero unresolved mismatches.

---

# Later — epics, NOT yet agent cards

| Epic | Why deferred |
|---|---|
| Artist portfolio (before/after galleries) | Part of the professional-network vision; needs entity + media-storage design first |
| Trend/analytics charts (revenue over time, retention) | Build after the basic rollups are proven; needs time-series query design |
| Real-time dashboard updates (push/websocket) | Infrastructure decision; out of scope until basics ship |
| Manager catalog/staff full CRUD screens | Separate from the dashboard rollup; cardable after C-01 lands |

---

## Quick reference — what the free agent must never own

- The SuperAdmin cross-tenant platform summary and its revenue aggregation (A-05).
- The `SavedSalon` tenant-owned-vs-global decision and its query-filter exemption (A-06).
- Any change to money math (only display formatting in B-07 is allowed).
- Any change to how role or tenant is resolved from auth (B-04 reads it; it must not author it).

For these the agent's only sanctioned output is read-only analysis or an agreed-model migration/DTO skeleton — then stop and escalate.
