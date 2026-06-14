# SalonOS — User Roles BASE STRUCTURE: Agent-Ready TODO

**For:** a free / local AI coding agent (e.g. Continue.dev + Ollama / deepseek-coder)
**Source:** `SALONOS_USER_ROLES_SPEC.md` (this is **phase 1** of it)
**Generated:** 2026-06-13
**Where it lives:** the new backend's Identity module (`src/Modules/Identity/…`).

**Goal:** stand up the *correct, extensible skeleton* of the role model — and nothing more. Build the parts that are **structural and expensive to change later**; stub or defer the parts that are **additive and cheap to add later** (the granular per-user abilities you'll specify next).

The test for "is it base structure?": *if getting it wrong now forces a painful data migration or security retrofit later, it's in the base. If it's just another field or a tighter rule that drops in cleanly later, defer it.*

---

## What's IN the base vs DEFERRED

**IN (build now — structural / irreversible):**
- One `ApplicationUser` + `UserType` discriminator.
- Profile entities with only their *structural* fields + correct tenancy shape.
- **Tenancy shape:** `TenantId` on Manager/Artist; **none** on Client/SuperAdmin.
- JWT carrying `userType` + `tenantId` (minimal claim set).
- **Coarse** role-based authorization (gate by role) — not the fine matrix.
- Tenant query-filter exemptions for the global roles.
- A SuperAdmin seed + a sanity test.

**DEFERRED (your next "abilities" pass — additive / cheap):**
- The full §8 capability matrix and per-endpoint ownership rules.
- Detailed profile fields (bio, specialties, working hours, national code + its encryption, gender, DOB…).
- `SavedSalon`/favorites and other features.
- SuperAdmin sub-permissions, chain support, portfolio, guest→client conversion.

The full spec (`SALONOS_USER_ROLES_SPEC.md`) is the blueprint for the deferred pass — nothing here blocks it; it all layers on top.

---

## How the agent must operate

Carry over the rules from the earlier TODO files, plus:
1. **Read `multi-tenancy/SKILL.md` first.** The tenancy shape is the whole point of this phase.
2. **`ClientProfile` (and SuperAdmin) get NO `TenantId` and NO tenant filter.** If a card seems to need one, **stop and report**.
3. **Keep profiles minimal.** Add only the fields each card lists. Do not pre-build the deferred fields — they come later via migration.
4. **Coarse, not granular.** Gate endpoints by *role* only. Do not write ownership logic ("does this manager own this salon") in this phase — that's the deferred pass.
5. **No `Assert.True(true)`** — the sanity test must fail if a role crosses a boundary.

## Delegation legend

| Flag | Meaning |
|---|---|
| 🟢 | Agent-safe. |
| 🟡 | Agent drafts; human reviews (tenancy fields, migrations, role wiring). |
| 🔴 | Do **not** delegate. Structural/security decision; Claude/human authors. |

---

## Master index

| ID | Task | Flag | Effort | Depends on |
|---|---|---|---|---|
| BASE-01 | Add `UserType` enum | 🟢 | S | — |
| BASE-02 | Minimal profile entities (Manager/Artist/Client) | 🟡 | M | BASE-01 |
| BASE-03 | Consolidate to one `ApplicationUser` | 🔴 | M | BASE-02 |
| BASE-04 | EF config + migration (tenancy shape) | 🟡 | M | BASE-02, BASE-03 |
| BASE-05 | JWT carries `userType` + `tenantId` | 🔴 | S | BASE-03 |
| BASE-06 | Coarse role-based authorization | 🟡 | M | BASE-05 |
| BASE-07 | Tenant filter exemptions (Client/SuperAdmin) | 🔴 | S | NOW-07, BASE-02 |
| BASE-08 | Seed a SuperAdmin | 🟡 | S | BASE-03 |
| BASE-09 | Sanity tests (1:1 + tenancy boundary) | 🔴 | S | BASE-06, BASE-07 |

**Sequencing:** BASE-01 → 02 → 03 (gate) → 04 → 05 (gate) → 06 → 07 (gate) → 08 → 09. The three 🔴 gates (03, 05, 07) are cleared by a human before the dependent work.

---

## Cards

### BASE-01 — `UserType` enum 🟢
**Files:** `src/Modules/Identity/Domain/Enums/UserType.cs`
**Steps:** Add `enum UserType { SuperAdmin, SalonManager, Artist, Client }`.
**Done when:** compiles; referenced by BASE-02. *(Other enums like ContractType/Gender are deferred.)*

### BASE-02 — Minimal profile entities 🟡
**Files:** `src/Modules/Identity/Domain/` — `SalonManagerProfile`, `ClientProfile`; extend `ArtistProfile`.
**Steps:** Each profile carries **only** structural fields for now:
- `SalonManagerProfile`: `UserId` (unique FK), `TenantId`, `SalonId`, `IsOwner`.
- `ArtistProfile`: `UserId` (unique FK), `TenantId`, `SalonId`.
- `ClientProfile`: `UserId` (unique FK) — **no `TenantId`.**
Defer bio/specialties/national code/loyalty/etc. to the abilities pass.
**Done when:** three profiles compile with exactly these fields.
**Review focus:** `ClientProfile` has no `TenantId`.

### BASE-03 — Consolidate to one `ApplicationUser` 🔴
**Why red:** unifying the two identity classes into one global `ApplicationUser` + `UserType` (spec §3 / P1-09) is structural and underpins every role. Claude/human authors the merged class + field-migration mapping. Agent may produce a field-by-field diff of the two existing classes, then stop.

### BASE-04 — EF config + migration 🟡
**Files:** `src/SalonOS.Infrastructure/Configurations/`, `…/Migrations/`
**Steps:** Configure each profile 1:1 with `ApplicationUser` (unique `UserId`); index `TenantId` on Manager/Artist; **no `TenantId`** mapping/column for `ClientProfile`. Generate and inspect the migration.
**Done when:** migration applies to a fresh DB; client table has no tenant column; no destructive drop of existing user data without a data-migration step.

### BASE-05 — JWT carries `userType` + `tenantId` 🔴
**Why red:** this is NOW-06, minimal form. The token must carry `userType` always and `tenantId` for Manager/Artist only (resolved from the profile, never from request input); SuperAdmin/Client tokens carry no `tenantId`. Claude/human authors.

### BASE-06 — Coarse role-based authorization 🟡
**Files:** `src/SalonOS.Api/` auth setup + controllers.
**Steps:** Register four coarse policies (or role requirements) keyed on `userType` — `Platform`, `SalonManage`, `ArtistSelf`, `ClientSelf` — and gate each controller by the role(s) allowed to use it. **Coarse only** — no resource-ownership checks yet.
**Done when:** a wrong-role caller gets 403; the right role passes; build green.
**Review focus:** role comes from the verified token claim (BASE-05), not from input. *(Granular per-capability rules + ownership = deferred pass.)*

### BASE-07 — Tenant filter exemptions 🔴
**Why red:** configures the global tenant filter (NOW-07) to **exempt** `ClientProfile` and SuperAdmin platform reads. Get this wrong and the client experience breaks silently. Claude/human authors. (No `SavedSalon` yet — it's deferred — but the Client exemption pattern is established here for it to reuse later.)

### BASE-08 — Seed a SuperAdmin 🟡
**Files:** seed/initialization in Identity module.
**Steps:** Idempotently seed one `SuperAdmin` (no tenant) from configuration. Public signup must not create this role.
**Done when:** fresh DB has exactly one SuperAdmin; re-running doesn't duplicate.

### BASE-09 — Sanity tests 🔴
**Why red:** two assertions that must be authored correctly: (1) each profile is 1:1 with a user; (2) a SalonManager/Artist cannot read another tenant's rows, while a Client is not tenant-filtered. Claude/human writes the assertions; agent may build the two-tenant fixture, then stop.
**Done when:** tests pass and fail correctly when the tenancy boundary is broken.

---

## How the deferred "abilities" pass layers on (so you build it right now)

- **Adding profile fields** → additive EF migration; the entity already exists. No rework.
- **Granular permissions** → split each coarse policy into capability policies and re-annotate endpoints against §8; the auth *mechanism* (BASE-06) already exists.
- **Ownership rules** → add authorization handlers (spec Z-03 / P1-07) on top of the coarse gate; endpoints already carry policies.
- **`SavedSalon`/favorites** → new entity reusing the Client global-exemption pattern from BASE-07.

Building the base this way means the abilities pass is *all additive* — no migration of identity or tenancy needed.

---

## Quick reference — what the free agent must never own

- The `ApplicationUser` consolidation + migration mapping (BASE-03).
- JWT claim issuance (BASE-05).
- Tenant filter exemptions for the global roles (BASE-07).
- The tenancy-boundary test assertions (BASE-09).

Agent's only sanctioned output on these: a diff, an agreed-model migration, a test fixture (not assertions) — then stop and escalate.
