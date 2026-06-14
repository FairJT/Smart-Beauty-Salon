# SalonOS — Roadmap V2 Implementation: Agent-Ready TODO

**For:** a free / local AI coding agent (e.g. Continue.dev + Ollama / deepseek-coder)
**Source:** `SALONOS_IMPLEMENTATION_ROADMAP_V2.md`
**Generated:** 2026-06-13
**Prerequisite:** Phase 0 (foundation + base roles) from `SALONOS_AGENT_TODO.md` and `SALONOS_ROLES_BASE_STRUCTURE_TODO.md` is done — especially tenant query filters and the `Money` type. Catalog rows are tenant-scoped; pricing uses `Money`. Without Phase 0 this is unsafe.

This file fully cards **Phase 1 (configurable catalog & salon identity)** — the buildable-now backbone. **Phase 2 (booking)** is given as coarse cards because it's blocked by open product decisions. **Phases 3–6** stay as epics until their prerequisites and decisions are resolved; they are not handed to a local model yet.

---

## Resolve before / during Phase 1

From roadmap §4, two sub-decisions affect Phase 1 cards directly (the others block Phase 2+):
- **Options & materials: platform templates or salon-defined?** Affects whether `ServiceOption`/`Material` are global lookups or tenant rows. The cards below assume **salon-defined (tenant-scoped)** with platform-defined service *types* — change them if you decide otherwise.
- **Salon pages: real subdomains or path-based?** (decision #4) — gates P1-12 only.

---

## How the agent must operate

Carry over all rules from the earlier TODO files, plus:
1. **Read both skills first** — `multi-tenancy/SKILL.md` (catalog is tenant-scoped) and `payments/SKILL.md` (prices are money).
2. **Money is integer minor units + currency.** Base prices, option deltas, material prices are all `Money`. **Never a float.** Durations are integer minutes.
3. **Do not write the pricing calculation.** Combining base + options + material into an estimate is money math (P1-05) — 🔴. The agent may scaffold the method signature only.
4. **Match the agreed global-vs-tenant model.** `ServiceType` is platform/global; `SalonService`/`ServiceOption`/`Material` are tenant-scoped. If a card seems to require the opposite, stop and report.
5. **No `Assert.True(true)`** — tenant-scoping and pricing tests must fail when behaviour is wrong.

## Delegation legend

| Flag | Meaning |
|---|---|
| 🟢 | Agent-safe. Mechanical; model is fixed. |
| 🟡 | Agent drafts; human reviews (money, tenancy, migrations). |
| 🔴 | Do **not** delegate. Money math, global-vs-tenant decisions, or security tests. Claude/human authors. |

---

## Master index — Phase 1

| ID | Task | Track | Flag | Effort | Depends on |
|---|---|---|---|---|---|
| P1-01 | `ServiceType` + `Category` (platform/global) | Catalog | 🔴 | S | Phase 0 |
| P1-02 | `SalonService` (tenant instance) + base price/duration | Catalog | 🟡 | M | P1-01 |
| P1-03 | `ServiceOption` (price/duration deltas) | Catalog | 🟡 | M | P1-02 |
| P1-04 | `Material` (tenant) + service link | Catalog | 🟡 | S | P1-02 |
| P1-05 | Estimate calculator (base + options + material) | Catalog | 🔴 | M | P1-03, P1-04 |
| P1-06 | Catalog CRUD endpoints + role gates | Catalog | 🟡 | M | P1-02..P1-04 |
| P1-07 | `ContractType` + employment fields on Artist | Staffing | 🟡 | M | Phase 0 |
| P1-08 | Extend `Salon` profile fields | Salon | 🟢 | S | — |
| P1-09 | `SalonMedia` gallery (image/video) | Salon | 🟡 | S | — |
| P1-10 | Salon theming (color/logo/font) | Salon | 🟢 | S | — |
| P1-11 | Migration for all Phase 1 entities | Catalog/Salon | 🟡 | S | P1-01..P1-10 |
| P1-12 | Salon page routing (subdomain/path) | Salon | 🔴 | M | decision #4 |
| P1-13 | Tests: scoping, pricing, contract type | QA | 🔴 | M | P1-11 |

**Sequencing:** P1-01 → 02 → (03, 04) → 05 in order (catalog spine). P1-07/08/09/10 run in parallel. P1-11 migration after the model settles. P1-12 only once decision #4 is made. P1-13 last.

---

## Phase 1 cards

### P1-01 — `ServiceType` + `Category` 🔴
**Why red:** this is a **platform/global** lookup (SuperAdmin defines allowed activities like manicure/haircut) — the global-vs-tenant call, like favorites, must be made deliberately so it is *not* caught by the tenant filter. Claude/human authors the entity + filter exemption. Agent may draft the field list (`Id`, `Name`, `Category`) only, then stop.
**Files (reference):** `src/Modules/Catalog/Domain/ServiceType.cs`.

### P1-02 — `SalonService` 🟡
**Effort:** M · **Depends:** P1-01
**Files:** `src/Modules/Catalog/Domain/SalonService.cs`, EF config.
**Steps:** A tenant's instance of a `ServiceType`. Fields: `Id`, `TenantId`, `SalonId`, `ServiceTypeId`, `BasePrice` (`Money`), `BaseDurationMinutes` (int), `IsActive`. Tenant-scoped (filter from Phase 0 applies).
**Done when:** compiles; `BasePrice` is `Money` (not a float); tenant filter scopes it.
**Review focus:** money type; `TenantId` present and indexed.

### P1-03 — `ServiceOption` 🟡
**Effort:** M · **Depends:** P1-02
**Files:** `src/Modules/Catalog/Domain/ServiceOption.cs`, EF config.
**Steps:** Belongs to a `SalonService` (tenant-scoped). Fields: `Id`, `TenantId`, `SalonServiceId`, `Name`, `PriceDelta` (`Money`), `DurationDeltaMinutes` (int). Example: haircut → "tip trim" (+price, +minutes).
**Done when:** compiles; deltas are `Money`/int; scoped to its service.

### P1-04 — `Material` 🟡
**Effort:** S · **Depends:** P1-02
**Files:** `src/Modules/Catalog/Domain/Material.cs`, link table to service.
**Steps:** Tenant-scoped material (e.g. polish brand) with `Price` (`Money`). Link materials to services they apply to.
**Done when:** compiles; `Price` is `Money`; tenant-scoped.
**Review focus:** confirm tenant (not global) per the agreed model.

### P1-05 — Estimate calculator 🔴
**Why red:** this sums `BasePrice` + selected option `PriceDelta`s + `Material.Price` into an estimate, and `BaseDuration` + option deltas into a total duration. Money aggregation with a currency-match guard is exactly what the payments skill says to author carefully — a weak model is the wrong tool. Claude/human authors. Agent may scaffold the method signature (`EstimateResult Calculate(SalonService, IEnumerable<ServiceOption>, Material?)`) and stop.

### P1-06 — Catalog CRUD endpoints 🟡
**Effort:** M · **Depends:** P1-02..P1-04
**Files:** `src/Modules/Catalog/…/Controllers/`.
**Steps:** CRUD for `SalonService`, `ServiceOption`, `Material` gated to `SalonManager` (own salon); `ServiceType` admin endpoints gated to `SuperAdmin`. Use the coarse role gate from Phase 0.
**Done when:** a manager manages only their own salon's catalog; a non-manager is 403; admin manages service types.
**Review focus:** role gates match the role model; tenant scoping holds.

### P1-07 — `ContractType` + employment fields 🟡
**Effort:** M · **Depends:** Phase 0
**Files:** `src/Modules/…/ArtistProfile` (or a `StaffContract` entity).
**Steps:** Add `ContractType { FixedSalary, LineRental }`. For `FixedSalary`: `Salary` (`Money`). For `LineRental`: rent terms. This field drives Phase 2 availability/leave/payroll — model it as first-class, not a flag buried elsewhere.
**Done when:** compiles; `Salary` is `Money`; the enum is queryable per artist.
**Review focus:** money type for salary; rate vs money distinction.

### P1-08 — Extend `Salon` profile 🟢
**Effort:** S · **Depends:** —
**Files:** `Salon` entity + config.
**Steps:** Add `License`, `Grade/Rank`, `Fax`, and meta fields. (Lat/long already exist.)
**Done when:** fields persist; build passes.

### P1-09 — `SalonMedia` gallery 🟡
**Effort:** S · **Depends:** —
**Files:** `src/Modules/Salon/Domain/SalonMedia.cs`.
**Steps:** Tenant-scoped media (image/video URL, type, order) belonging to a salon.
**Done when:** a salon can hold an ordered gallery; tenant-scoped.

### P1-10 — Salon theming 🟢
**Effort:** S · **Depends:** —
**Files:** theming fields on `Salon` or a `SalonTheme` entity.
**Steps:** `PrimaryColor`, `LogoUrl`, `FontColor`, editable by the manager (applied to the shared template).
**Done when:** fields persist and are readable for rendering the salon page.

### P1-11 — Migration 🟡
**Effort:** S · **Depends:** P1-01..P1-10
**Files:** `src/SalonOS.Infrastructure/Migrations/`.
**Steps:** One migration adding all Phase 1 tables/columns. Inspect SQL. `ServiceType` has no `TenantId`; tenant entities do (indexed).
**Done when:** applies cleanly to a fresh DB; global vs tenant columns are correct.

### P1-12 — Salon page routing 🔴
**Why red:** subdomain routing (DNS/wildcard + middleware resolving tenant from host) vs path-based (`/salon/{slug}`) is an infra + tenant-resolution decision (#4). Tenant resolution from the host is security-sensitive. Claude/human decides and authors. Deferred until decision #4 is made.

### P1-13 — Tests 🔴
**Why red:** the assertions that matter are security/correctness: (1) a salon cannot read another salon's services/options/materials; (2) the estimate calculator returns the right total for a known base+options+material; (3) contract type persists and is queryable. Money and tenant-boundary assertions must be authored correctly. Claude/human writes assertions; agent may build fixtures, then stop.

---

## Phase 2 — Booking engine (COARSE cards — blocked on decisions)

The heart of the product, but blocked by roadmap §4 decisions (#3 slot duration, plus #1/#2 for related behaviour) and by Phase 1. Do **not** fine-decompose or hand to the agent until those are resolved and Phase 1 is done. Coarse shape:

| Epic card | Note / blocker |
|---|---|
| Availability service branching on contract type | Needs P1-07; salon-hours vs artist-calendar logic — 🔴 |
| Leave/holiday blocking slots | Needs availability; approval rules depend on contract type |
| Three booking entry points | Dashboard search / salon page / artist page |
| Booking flow: options → estimate → deposit → reserve | Uses P1-05 (🔴 money) + deposit + slot lock (concurrency 🔴) |
| Fixed vs variable slot duration | **Blocked on decision #3** |
| In-service upsell / service change | After base booking works |

When decisions #1–#3 are answered and Phase 1 ships, this becomes its own fully-carded TODO.

---

## Phases 3–6 — Epics, NOT yet agent cards

| Phase | Epic | Why deferred |
|---|---|---|
| 3 | Bidirectional reviews + moderation | Blocked on decision #1 (public/approval) |
| 3 | Artist profiles (resume, skills %, certificates) | Cardable after Phase 1; mostly 🟡 |
| 3 | Public pages (home/salon/artist) + 5 dashboards | Needs catalog + booking; folds in `SALONOS_UI_DASHBOARD_TODO.md` |
| 4 | Job board (JobSeeker ↔ Manager) | Needs JobSeeker capability; pay-per-application = decision #5 |
| 5 | Accounting + payroll | 🔴 money math throughout — stays off the local model |
| 5 | Monetization + CMS (ads/ladder/VIP/blog) | Platform features; design first |
| 6 | AI (LLM search + consultation) | Final version; needs rich catalog + history |

---

## Quick reference — what the free agent must never own (Phase 1)

- `ServiceType` global-vs-tenant decision + filter exemption (P1-01).
- The estimate calculator's money/duration math (P1-05).
- Salon page tenant-from-host routing (P1-12).
- Tenant-scoping and pricing test assertions (P1-13).
- Anything in Phase 2's availability/booking-lock/money paths.

For these the agent's only sanctioned output is a field-list draft, a method signature, a migration from an agreed model, or a test fixture — then stop and escalate.
