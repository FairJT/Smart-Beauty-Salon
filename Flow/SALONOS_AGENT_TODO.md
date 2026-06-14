# SalonOS — Agent-Ready TODO List

**For:** a free / local AI coding agent (e.g. Continue.dev + Ollama / deepseek-coder)
**Source:** `SALONOS_DESIGN_AND_IMPLEMENTATION_PLAN.md`
**Generated:** 2026-06-13

This breaks the near-term plan into tasks small enough for a local agent to execute one at a time. Each card lists exact files, concrete steps, and a verifiable "Done when". Tasks the agent must **not** own are flagged and explained — hand those to Claude or a human.

Only the **Now** and **Phase 1** work (plus two concrete Phase 2 items) is decomposed to card level. Marketplace, Professional Network, Accounting, and Inventory features need design before they can be cut into agent cards — they are listed as epics at the end, not handed to a local model yet.

---

## How the agent must operate (read this first)

1. **One card at a time.** Do not start the next card until the current card's "Done when" passes.
2. **Read the skill first.** Before any task whose card mentions *tenant, salon, money, payment, currency, payout, or auth*, open and follow `/mnt/skills/user/multi-tenancy/SKILL.md` and/or `/mnt/skills/user/payments/SKILL.md`.
3. **Stay in scope.** Edit only the files listed on the card. If you think you need to touch another file, **stop and report** — do not improvise.
4. **Verify every change.** After editing, build the affected project and run the command named in "Done when". Paste the output.
5. **Never write a fake-passing test.** A test must fail when the behavior is wrong. `Assert.True(true)` and equivalents are forbidden — assert real values.
6. **No thrashing.** If a build or test fails twice, stop and report the exact error. Do not keep guessing.
7. **Respect the red line.** If a card is flagged 🔴, do not implement it. Scaffold only if the card says so, then stop and escalate.
8. **Small commits.** One logical change per commit, message prefixed with the task ID (e.g. `NOW-04: replace DateTime.Now with UtcNow`).

## Delegation legend

| Flag | Meaning |
|---|---|
| 🟢 | Agent-safe. Mechanical, low-risk, single-file-ish. |
| 🟡 | Agent-with-review. Agent drafts; a human reviews before merge. |
| 🔴 | Do **not** delegate. Claude/human authors the logic. Agent may only scaffold structure if the card explicitly says so. |

---

## Master index

| ID | Task | Phase | Flag | Effort | Depends on |
|---|---|---|---|---|---|
| NOW-01 | Move SA password & JWT key out of source | Now | 🟡 | S | — |
| NOW-02 | Replace `EnsureCreated()` with `Migrate()` (both backends) | Now | 🟢 | S | — |
| NOW-03 | Delete leftover `Program.cs.cs` + write README | Now | 🟢 | S | — |
| NOW-04 | `DateTime.Now` → `DateTime.UtcNow` | Now | 🟢 | S | — |
| NOW-05 | Add global exception handler to new backend | Now | 🟡 | S | — |
| NOW-06 | Add `TenantId` to JWT claims | Now | 🔴 | S | — |
| NOW-07 | Wire EF Core global query filters (tenancy Layer 2) | Now | 🔴 | M | NOW-06 |
| NOW-08 | Replace placeholder cross-tenant isolation tests | Now | 🔴 | M | NOW-07 |
| P1-01 | Production CORS policy (new backend) | P1 | 🟡 | S | — |
| P1-02 | Register `OutboxMessage` DbSet + attach interceptor | P1 | 🟡 | S | NOW-02 |
| P1-03 | Register Hangfire outbox dispatcher job | P1 | 🟡 | S | P1-02 |
| P1-04 | Migrate `ReminderService` to a Hangfire recurring job | P1 | 🟡 | M | P1-03 |
| P1-05 | Per-module FluentValidation validators (new backend) | P1 | 🟢 | M | — |
| P1-06 | Per-user login rate limiting | P1 | 🟡 | S | — |
| P1-07 | Salon-ownership checks on CRUD endpoints | P1 | 🟡 | M | NOW-06 |
| P1-08 | Implement Zarinpal adapter behind `IPaymentProvider` | P1 | 🔴 | M | — |
| P1-09 | Consolidate duplicate `ApplicationUser` model | P1 | 🔴 | M | NOW-07 |
| P1-10 | Token → `flutter_secure_storage` (Flutter) | P1 | 🟡 | S | — |
| P1-11 | Top-level error boundary (Flutter) | P1 | 🟢 | S | — |
| P1-12 | Build Flutter web inside the Docker image | P1 | 🟢 | S | — |
| P1-13 | CI pipeline (build + test + lint) | P1 | 🟡 | M | tests exist |
| P2-01 | Extract Persian strings to `intl` ARB catalogs | P2 | 🟢 | M | — |
| P2-02 | Add soft-delete + audit columns (migration) | P2 | 🟡 | M | NOW-02 |

---

# Phase: Now

### NOW-01 — Move SA password & JWT key out of source 🟡
**Effort:** S · **Depends:** —
**Files:** `docker-compose.yml`, `appsettings.json`, `.env.example` (new), `.gitignore`
**Steps:**
1. Replace the literal `MSSQL_SA_PASSWORD` in `docker-compose.yml:14` with `${MSSQL_SA_PASSWORD}`.
2. Do the same for the JWT key reference in `appsettings.json:10` (read from env var `JWT_SECRET`).
3. Create `.env.example` listing the variable names with empty values (no real secrets).
4. Ensure `.env` is in `.gitignore`.
**Done when:** `docker compose config` resolves variables from a local `.env`; no secret literals remain in tracked files (`git grep -i password docker-compose.yml` returns nothing).
**Human action (not the agent):** rotate the previously committed password — it is considered leaked.

### NOW-02 — Replace `EnsureCreated()` with `Migrate()` 🟢
**Effort:** S · **Depends:** —
**Files:** `src/SalonOS.Api/Program.cs` (line ~108), `SmartSalon/SmartSalon/Program.cs` (line ~196)
**Steps:** Replace each `Database.EnsureCreated()` call with `Database.Migrate()`.
**Done when:** both projects build; on a fresh DB the app applies migrations on startup without error.

### NOW-03 — Remove leftover file + write README 🟢
**Effort:** S · **Depends:** —
**Files:** `SmartSalon/SmartSalon/Program.cs.cs` (delete), `README.md`
**Steps:** Delete the duplicate `Program.cs.cs`. Write a README covering: what the project is, the two backends, prerequisites, and how to run via `docker compose up`.
**Done when:** duplicate file gone; README renders and the run command listed actually starts the stack.

### NOW-04 — `DateTime.Now` → `DateTime.UtcNow` 🟢
**Effort:** S · **Depends:** —
**Files:** `SmartSalon/SmartSalon/Services/AppointmentService.cs` (~line 207), `SmartSalon/SmartSalon/Services/ReminderService.cs` (~line 37)
**Steps:** Replace `DateTime.Now` with `DateTime.UtcNow` at both sites. Check no other `DateTime.Now` remains: `git grep "DateTime.Now" -- "*.cs"`.
**Done when:** build passes; grep returns no `DateTime.Now` in service code.

### NOW-05 — Global exception handler (new backend) 🟡
**Effort:** S · **Depends:** —
**Files:** `src/SalonOS.Api/Program.cs`
**Steps:** Add `app.UseExceptionHandler(...)` (or an `IExceptionHandler`) that returns a clean ProblemDetails JSON and logs the error. Mirror the pattern already used in the legacy backend.
**Done when:** an endpoint that throws returns a structured 500 (no stack trace in the body) and the error is logged.

### NOW-06 — Add `TenantId` to JWT claims 🔴
**Effort:** S · **Depends:** —
**Why red:** auth-claim changes are security-critical and underpin all tenant isolation. Claude/human authors this; the agent may read the code and report where claims are issued, nothing more.
**Files (for reference only):** the auth/token-issuing service in the Identity module.

### NOW-07 — Wire EF Core global query filters (tenancy Layer 2) 🔴
**Effort:** M · **Depends:** NOW-06
**Why red:** this is the platform's core security guarantee; the commented-out filters in `AppDbContext.cs` (lines ~30–41) and the stubbed `ApplyTenantFilter<T>` must be authored against the multi-tenancy skill, not guessed. A subtle mistake here is a cross-tenant data breach.
**Files (for reference only):** `src/SalonOS.Infrastructure/AppDbContext.cs`.

### NOW-08 — Replace placeholder cross-tenant isolation tests 🔴
**Effort:** M · **Depends:** NOW-07
**Why red:** the four tests in `tests/SalonOS.Tenancy.Tests/TenantIsolationTests.cs` currently assert `Assert.True(true)`. The replacement must follow the mandatory shape in the multi-tenancy skill — seed tenant A and B, authenticate as A, attempt to read **and** mutate B's rows by id, assert both fail. A weak model is exactly the wrong tool for a test whose whole point is to fail correctly. Claude/human writes the assertions; the agent may set up the test fixtures/builders only if asked, then stop.

---

# Phase 1

### P1-01 — Production CORS policy 🟡
**Effort:** S · **Depends:** —
**Files:** `src/SalonOS.Api/Program.cs` (~line 121)
**Steps:** Add a `Production` CORS policy reading allowed origins from config; keep `AllowAll` only for the Development environment. Select policy by `IHostEnvironment`.
**Done when:** in Production env the API rejects a disallowed origin; in Development it still allows local clients.

### P1-02 — Register `OutboxMessage` DbSet + attach interceptor 🟡
**Effort:** S · **Depends:** NOW-02
**Files:** `src/SalonOS.Infrastructure/AppDbContext.cs`, EF config
**Steps:** Add `DbSet<OutboxMessage>`, register `OutboxInterceptor` via `AddInterceptors`, add a migration for the `OutboxMessage` table with an index on `(ProcessedAt, OccurredAt)` (or equivalent).
**Done when:** migration applies; saving an entity that raises a domain event writes an `OutboxMessage` row in the same transaction.

### P1-03 — Register Hangfire outbox dispatcher job 🟡
**Effort:** S · **Depends:** P1-02
**Files:** `src/SalonOS.Api/Program.cs`, `src/SalonOS.Infrastructure/HangfireJobDispatcher.cs`
**Steps:** Register a recurring job `ProcessOutboxMessages` (~every 5s) that reads unprocessed outbox rows, dispatches them, and marks them processed.
**Done when:** an unprocessed outbox row is picked up and marked processed within a few seconds; the Hangfire dashboard shows the recurring job.

### P1-04 — Migrate `ReminderService` to Hangfire recurring job 🟡
**Effort:** M · **Depends:** P1-03
**Files:** new job class in the Booking module/infrastructure; remove/disable the legacy `BackgroundService` once parity is confirmed.
**Steps:** Re-implement the hourly reminder logic (confirmed appointments ~2h out → SMS + in-app notification → mark `ReminderSent`) as a Hangfire recurring job using `UtcNow`.
**Done when:** the job runs on schedule and marks reminders sent; behavior matches the legacy service.

### P1-05 — Per-module FluentValidation validators 🟢
**Effort:** M · **Depends:** —
**Files:** `Application/` folders of Booking, Catalog, Inventory modules
**Steps:** For each request DTO that lacks one, add a `AbstractValidator<T>` covering required fields, ranges, and string lengths. Register them so `AddFluentValidationAutoValidation()` actually has validators to run.
**Done when:** invalid requests return 400 with field errors; build passes.
**Note:** booking/price validators that touch money must use the `Money` type rules — if unsure, flag for review.

### P1-06 — Per-user login rate limiting 🟡
**Effort:** S · **Depends:** —
**Files:** rate-limiter config in `Program.cs` (legacy and/or new)
**Steps:** Add a limiter partitioned by username/identity (not only IP) on the login endpoint, stricter than the general `auth` policy.
**Done when:** repeated failed logins for one account are throttled independently of IP.

### P1-07 — Salon-ownership checks on CRUD endpoints 🟡
**Effort:** M · **Depends:** NOW-06
**Files:** `SalonsController`, `ServicesController` (and any endpoint flagged in the report's §6.3.3)
**Steps:** Before mutating a salon/service, verify the caller's resolved tenant/salon owns the target row. Reject with 403 otherwise. Use the resolved tenant context — never a `salonId` from the request body.
**Done when:** a manager cannot edit/delete a salon or service belonging to another tenant (covered by a test); legitimate owner operations still succeed.
**Note:** keep ownership resolution reading from context per the multi-tenancy skill; flag for review.

### P1-08 — Implement Zarinpal adapter behind `IPaymentProvider` 🔴
**Effort:** M · **Depends:** —
**Why red:** payment + webhook code is correctness- and security-critical (idempotency keys, signature-verified webhooks, gateway-state-authoritative) per the payments skill. The agent must not author this. It may, if asked, generate the adapter's *interface-conformance skeleton* (empty methods matching `IPaymentProvider`) and stop.
**Files (for reference only):** `src/SalonOS.Infrastructure/Payments/ZarinpalProvider.cs`.

### P1-09 — Consolidate duplicate `ApplicationUser` 🔴
**Effort:** M · **Depends:** NOW-07
**Why red:** merging the two identity models (`SmartSalon.Models.ApplicationUser` vs `SalonOS.Identity.Domain.ApplicationUser`) is a cross-cutting decision affecting auth, migrations, and both backends. Needs whole-repo judgment. Agent may produce a field-by-field diff of the two classes to inform the decision, nothing more.

### P1-10 — Token → `flutter_secure_storage` 🟡
**Effort:** S · **Depends:** —
**Files:** Flutter auth provider/storage layer (`providers/auth_provider.dart` and the Dio interceptor that reads the token)
**Steps:** Add `flutter_secure_storage`; replace `shared_preferences` reads/writes of the auth token with secure storage. Leave non-sensitive prefs as-is.
**Done when:** login persists the token to secure storage; the Dio interceptor reads it from there; app still authenticates after restart.

### P1-11 — Top-level error boundary (Flutter) 🟢
**Effort:** S · **Depends:** —
**Files:** `main.dart`
**Steps:** Add a global error handler (`FlutterError.onError` + a guarded zone / `ErrorWidget.builder`) that shows a friendly fallback instead of a crash.
**Done when:** a thrown error in a screen shows the fallback UI rather than a red error screen / crash.

### P1-12 — Build Flutter web inside the Docker image 🟢
**Effort:** S · **Depends:** —
**Files:** the Flutter web Dockerfile
**Steps:** Add a build stage that runs `flutter build web` inside the image, then serves the output with Nginx — so the build no longer depends on a pre-existing `build/web`.
**Done when:** a clean checkout builds the web image with no manual pre-build step.

### P1-13 — CI pipeline (build + test + lint) 🟡
**Effort:** M · **Depends:** tests exist
**Files:** `.github/workflows/ci.yml` (new)
**Steps:** On push/PR: restore + build both backends; `dotnet test`; `flutter analyze` + `flutter test`; fail the job on any error. Add image build on tag (no deploy yet).
**Done when:** the workflow runs green on a clean branch and red when a test is broken on purpose.

---

# Phase 2 (concrete, agent-decomposable items only)

### P2-01 — Extract Persian strings to `intl` ARB catalogs 🟢
**Effort:** M · **Depends:** —
**Files:** Flutter UI files with hardcoded strings; new `lib/l10n/*.arb`
**Steps:** Move hardcoded Persian UI strings into an ARB catalog and reference them through generated localizations. Do this screen-by-screen to keep diffs small (one screen per commit).
**Done when:** target screens render from the catalog; `flutter analyze` passes; no behavioral change.

### P2-02 — Soft-delete + audit columns 🟡
**Effort:** M · **Depends:** NOW-02
**Files:** new EF migration; `AppDbContext` query-filter config
**Steps:** Add `IsDeleted` (+ `DeletedAt`) and audit columns (`CreatedAt`/`UpdatedAt`) where missing; add a global query filter excluding soft-deleted rows; change delete operations on chosen entities to set the flag.
**Done when:** migration applies; "deleted" rows are hidden from normal queries but retained in the table.
**Note:** the soft-delete query filter must compose with the tenant filter from NOW-07 — flag for review so the two filters don't conflict.

---

# Later phases — epics, NOT yet agent cards

These are from Sections C and F of the plan. They require design (entities, APIs, money/payout flows, search ranking) before they can be cut into agent-sized tasks. **Do not hand these to a free agent** — they involve cross-tenant search, payouts, ledgers, and money math, all of which sit on the red line. Sequence and ownership:

| Epic | Phase | Owner before decomposition |
|---|---|---|
| Inventory & warehouse (extend existing skeleton) | P2–P3 | Design with Claude/human; then 🟡 cards for CRUD/UI, 🔴 for valuation |
| Marketplace (search, commission, payouts) | P3 | Claude/human (cross-tenant read + payouts are 🔴) |
| Professional network (profiles, feed, jobs) | P3 | Mostly 🟡 once designed; moderation/privacy reviewed |
| Accounting (ledger, invoicing, payroll, tax) | P4 | 🔴 throughout — money math stays off the local model |
| RLS (SQL Server `SESSION_CONTEXT`) / Postgres migration | P2–P3 | Claude/human |
| API versioning before marketplace clients | P3 | 🟡 |
| Backups + observability + TLS (DevOps, Section B) | P1–P2 | 🟡 infra cards; secrets/keys reviewed |

---

## Quick reference — what the free agent must never own

- Tenant query filters and the isolation logic (NOW-07) and their tests (NOW-08).
- JWT/auth claim changes (NOW-06).
- Payment adapter, webhook verification, idempotency (P1-08).
- Money representation/migration and any payroll/ledger/tax math (Accounting epic, parts of P2-02 review).
- The identity-model consolidation decision (P1-09).

For these, the agent's only sanctioned output is read-only analysis (diffs, "where is X defined") or empty interface-conformant skeletons when a card explicitly allows it — then stop and escalate.
