# fair — Fix-All Agent TODO

**For:** a free / local AI coding agent (e.g. Continue.dev + Ollama / deepseek-coder)
**Source:** review of `SALONOS_ROADMAP_V2_COMPLETION_REPORT.md`
**Generated:** 2026-06-14

These cards fix the issues found in the completion-report review. **Important:** the findings come from a status report, not verified code — so **every card begins by confirming the problem reproduces in the actual repo.** If a card's problem doesn't reproduce, report that and stop; don't invent a change.

---

## How the agent must operate

Carry over the rules from the earlier TODO files, plus:
1. **Verify before fixing.** Each card starts with a check. No reproduction → report and stop.
2. **Read both skills first** — `multi-tenancy/SKILL.md` and `payments/SKILL.md`. These fixes touch tenancy, IDs, money, and authorization.
3. **The `int SalonId` is never an authorization key.** Tenant scoping uses the `Guid TenantId` only. If you find int used for any scoping/auth decision, stop and report.
4. **Money math and authorization logic are 🔴.** Don't author them; scaffold only where a card allows.
5. **No `Assert.True(true)`.** Tests must fail when behaviour is wrong.
6. **Verify-then-delete.** Never delete code without first proving it's unreferenced.

## Delegation legend

| Flag | Meaning |
|---|---|
| 🟢 | Agent-safe. |
| 🟡 | Agent drafts; human reviews (migrations, tenancy, IDs, data wiring). |
| 🔴 | Do **not** delegate. Money, authorization, or identity-strategy decisions. Claude/human authors. |

---

## Master index

| ID | Task | Priority | Flag | Effort | Depends |
|---|---|---|---|---|---|
| FAIR-01 | Audit & remove all `EnsureCreated()` | Critical | 🟡 | S | — |
| FAIR-02 | Generate real EF migrations for Phase 1 entities | Critical | 🟡 | M | FAIR-01 |
| FAIR-03 | Compile + `flutter analyze` + fix frontend errors | High | 🟡 | M | — |
| FAIR-04 | Unit tests for the estimate calculator (money) | High | 🔴 | M | — |
| FAIR-05 | Authorization-enforcement tests | High | 🔴 | M | FAIR-08 |
| FAIR-06 | Public identifier strategy (slug vs int) | High | 🔴 | M | — |
| FAIR-07 | Audit: `int SalonId` never used for scoping | High | 🟡 | S | — |
| FAIR-08 | Audit: every endpoint has an explicit auth gate | High | 🟡 | S | — |
| FAIR-09 | Replace action-permission-as-role-proxy | Med-High | 🔴 | M | FAIR-08 |
| FAIR-10 | Remove dead legacy Flutter `screens/` | Medium | 🟡 | S | — |
| FAIR-11 | Favorites display-field staleness handling | Medium | 🟡 | M | — |
| FAIR-12 | Add `JobSeekerProfile` capability hook on Client | Low | 🟡 | S | — |
| FAIR-13 | Wire salon rating / reviewCount aggregation | Medium | 🟡 | M | — |
| FAIR-14 | Wire manager dashboard active service/artist counts | Medium | 🟡 | S | — |
| FAIR-15 | Resolve `ClientDashboard.favoriteSalons` mismatch | Low | 🟢 | S | — |
| FAIR-16 | Implement Zarinpal behind `IPaymentProvider` | Critical | 🔴 | M | — |

**Sequencing:** FAIR-01→02 (schema) and FAIR-07/08 (security audits) first — they de-risk everything else. Then the 🔴 gates (04, 05, 06, 09, 16) by a human. The 🟡/🟢 data and cleanup fixes can run anytime.

---

## Cards

### FAIR-01 — Audit & remove all `EnsureCreated()` 🟡
**Priority:** Critical
**Check:** `grep -ri "EnsureCreated" --include=*.cs` across both backends.
**Steps:** Confirm the contradiction (NOW-02 says it was replaced; P1-11 says it's still used to seed Phase 1). Remove every `EnsureCreated()` call; ensure startup uses `Database.Migrate()` only. Move any seeding to run *after* migration.
**Done when:** no `EnsureCreated()` remains; a fresh DB is built entirely by migrations.

### FAIR-02 — Generate real EF migrations for Phase 1 entities 🟡
**Priority:** Critical · **Depends:** FAIR-01
**Check:** `dotnet ef migrations list` — confirm whether migrations for `ServiceType`, `SalonService`, `ServiceOption`, `Material`, artist `ContractType`/`Salary`, `SalonMedia`, theming, `SavedSalon`, and `Tenant.SalonId` exist.
**Steps:** For any missing entity, generate a real EF migration. Inspect the generated SQL. Confirm `ServiceType` has no `TenantId`; tenant entities do (indexed); `SavedSalon` has no `TenantId`.
**Done when:** migrations apply cleanly to a fresh DB and contain all Phase 1 tables/columns; no "conceptual"/manual schema creation remains.
**Review focus:** no destructive drop of existing data without a data-migration step.

### FAIR-03 — Compile + analyze the Flutter app 🟡
**Priority:** High · *(requires Flutter SDK)*
**Check:** run `flutter pub get`, `flutter analyze`, `flutter build` (web), `flutter test`.
**Steps:** Fix compile and analyzer errors surfaced (Sections 6–8 were written but never compiled). Don't refactor beyond making it build cleanly.
**Done when:** `flutter analyze` is clean and the app builds; existing tests (if any) pass.
**Note:** if no Flutter SDK is available in the agent's environment, stop and report — this card needs one.

### FAIR-04 — Unit tests for the estimate calculator 🔴
**Priority:** High
**Why red:** these assert correct *money* totals (base + option deltas + material) and durations — money-correctness assertions per the payments skill. Claude/human authors the expected values + currency-mismatch case. Agent may build the test harness/fixtures (sample service + options + material), then stop.

### FAIR-05 — Authorization-enforcement tests 🔴
**Priority:** High · **Depends:** FAIR-08
**Why red:** assert that each role gets the right allow/deny on dashboard, favorites, and catalog endpoints. Security assertions must be authored correctly. Agent may build the role/endpoint fixture grid; Claude/human writes the assertions.

### FAIR-06 — Public identifier strategy 🔴
**Priority:** High
**Why red:** decides whether the public/Flutter salon identifier is the existing **slug** (recommended — non-enumerable, matches the subdomain design) or the auto-increment `int` (enumerable; leaks tenant count/growth, enables IDOR probing). This is an identity-model decision spanning backend + Flutter + favorites + URLs. Claude/human decides and authors the change. Agent may, once decided, mechanically apply the rename across the public endpoints and Flutter, then stop. **The `Guid TenantId` remains the sole isolation key regardless.**

### FAIR-07 — Audit: `int SalonId` never used for scoping 🟡
**Priority:** High
**Check:** trace every use of `SalonId` (int). Confirm none is used in a tenant filter, authorization check, or ownership decision (those must use `Guid TenantId`).
**Steps:** Produce a short report listing each `SalonId` usage and whether it's display/lookup (OK) or scoping (must change). Flag any scoping use — do not fix it here, report it.
**Done when:** a usage report exists and any scoping misuse is flagged.

### FAIR-08 — Audit: every endpoint has an explicit auth gate 🟡
**Priority:** High
**Check:** list every controller action and its `[Authorize]`/`[HasPermission]`/`[AllowAnonymous]` attribute.
**Steps:** Produce a table of endpoint → gate. Flag any action with no explicit gate (unauthenticated by omission). Confirm public endpoints (`GET /api/salons`) are intentionally anonymous.
**Done when:** the table is complete with zero "no gate by omission" rows unaccounted for.

### FAIR-09 — Replace action-permission-as-role-proxy 🔴
**Priority:** Med-High · **Depends:** FAIR-08
**Why red:** the client dashboard and favorites are gated on `AppointmentCreate` (an action permission used to mean "is a client"). Introduce intent permissions (e.g. `ClientSelf`) and remap. Authorization-semantics change → Claude/human authors the new permissions + mapping. Agent may draft the permission *constants*, then stop.

### FAIR-10 — Remove dead legacy Flutter `screens/` 🟡
**Priority:** Medium
**Check:** confirm `screens/` is imported nowhere (`grep -r "screens/" lib/ --include=*.dart`, check `main.dart`).
**Steps:** If unreferenced, delete the legacy Provider-based `screens/` directory. If anything still imports it, stop and report instead of deleting.
**Done when:** `screens/` is gone and the app still builds/analyzes clean (re-run FAIR-03 checks).

### FAIR-11 — Favorites display-field staleness 🟡
**Priority:** Medium
**Check:** confirm `SavedSalon` stores copied `SalonName`/`LogoUrl`.
**Steps:** Add a staleness strategy — refresh name/logo on read from the (global-exempt) salon lookup, or sync via the outbox on salon update. Don't reintroduce a tenant-filtered join that would hide cross-tenant salons.
**Done when:** a renamed salon's favorites show the current name; cross-tenant favorites still list correctly.
**Review focus:** the salon read path doesn't get caught by the tenant filter.

### FAIR-12 — Add `JobSeekerProfile` capability hook 🟡
**Priority:** Low
**Steps:** Add an opt-in 1:1 `JobSeekerProfile` on the Client (global — no `TenantId`), with placeholder fields (resume, skills, location). **Entity + relation only — no job-board endpoints/features** (that's Phase 4). This is the roadmap §2 hook so the later job board attaches cleanly.
**Done when:** a Client can have an optional `JobSeekerProfile`; migration applies; no tenant column on it.

### FAIR-13 — Wire salon rating / reviewCount aggregation 🟡
**Priority:** Medium
**Check:** confirm `GET /api/salons` returns `rating: 0, reviewCount: 0`.
**Steps:** Add an aggregation (query or maintained counter) for average rating + review count per salon, and populate the response.
**Done when:** `GET /api/salons` returns real rating/review figures.

### FAIR-14 — Wire manager dashboard active counts 🟡
**Priority:** Medium
**Check:** confirm `activeServiceCount`/`activeArtistCount` return 0.
**Steps:** Add queries (active services from Catalog, active artists from Identity) scoped to the manager's tenant, and populate the manager dashboard response.
**Done when:** the manager dashboard shows correct active counts for the caller's own salon.

### FAIR-15 — Resolve `ClientDashboard.favoriteSalons` mismatch 🟢
**Priority:** Low
**Steps:** Favorites come from a separate endpoint, so either remove the unused `favoriteSalons` field from the client dashboard model/DTO, or populate it from the favorites source — pick one and make backend + Flutter model consistent.
**Done when:** the field is either gone on both sides or populated; no dead/empty field remains.

### FAIR-16 — Implement Zarinpal behind `IPaymentProvider` 🔴
**Priority:** Critical (functional blocker)
**Why red:** payment + webhook code (idempotency keys, signature-verified idempotent webhooks, gateway-state-authoritative) per the payments skill. Still scaffold-only and correctly **not** delegated. Claude/human implements. Agent leaves the existing interface skeleton untouched.

---

## Quick reference — what the free agent must never own

- The estimate-calculator and authorization test *assertions* (FAIR-04, FAIR-05) — fixtures only.
- The public-identifier strategy decision (FAIR-06) — execute only after a human decides.
- The permission remap semantics (FAIR-09) — constants only.
- The Zarinpal payment/webhook implementation (FAIR-16).
- Any change that would use `int SalonId` as a scoping/auth key, or reintroduce `EnsureCreated()`.

For these, the agent's sanctioned output is a verification report, a test fixture, drafted constants, or a migration from an agreed model — then stop and escalate.
