# fair — Flutter ID Strategy & Fixes (revised)

**Supersedes:** `Flow/FAIR-remaining-flutter-todos.md` (the agent's draft)
**Generated:** 2026-06-14

The agent's draft was right to standardize Flutter on String IDs, but it (a) made the FAIR-06 public-identifier decision unilaterally, (b) left `int` exceptions that leak the internal key, and (c) was Flutter-only, so the backend keeps emitting `int`. This revision locks one identifier policy, applies it uniformly across backend *and* Flutter, and adds the performance and flexibility work.

## What the draft got right (keep)
- Standardizing Flutter entity IDs on String.
- The `/salon/{slug}/...` sub-resource endpoints.
- The list of files/fields that need touching.

## What it got wrong (fix)
- "Keep `FavoriteSalon.salonId` as int (legacy)" and "admin: no change, legacy int" — these **enshrine the internal-key leak**. Remove the int from all API surfaces.
- Three ID schemes coexisting (int + Guid + slug) with no policy.
- Per-repository fallback JSON parsing (`startsAt`/`startTime`) — fragile; centralize it.
- IDs spread across 15 files as raw `int`/`String` — no single source of truth, so every future change is another 15-file sweep.

---

## Operating rules
Carry over prior rules. Plus: **STRAT-01 is the FAIR-06 decision — a human locks it before any sweep.** Read `multi-tenancy/SKILL.md` (the internal `int` must never become an external/scoping key). No int may appear in any API request/response after this work.

| Flag | Meaning |
|---|---|
| 🟢 agent-safe · 🟡 agent drafts, human reviews · 🔴 do not delegate | |

---

## Master index

| ID | Task | Priority | Flag | Effort |
|---|---|---|---|---|
| STRAT-01 | Lock the identifier policy (one decision) | Critical | 🔴 | S |
| BE-01 | Stop exposing internal `int` in API responses | Critical | 🟡 | M |
| BE-02 | Unique index on salon `slug`; single-lookup resolution | High (perf) | 🟡 | S |
| BE-03 | Migrate `SavedSalon` to store salon **slug** | High | 🟡 | M |
| BE-04 | Verify clustered-PK strategy on hot tables | Medium (perf) | 🔴 | M |
| FE-01 | Centralize ID types (aliases / one module) | High (flex) | 🟡 | S |
| FE-02 | Sweep Flutter to String IDs — no exceptions | High | 🟡 | M |
| FE-03 | Standardize salon sub-resource URLs on `/salon/{slug}/...` | Medium | 🟡 | S |
| FE-04 | Centralize JSON mapping; drop per-repo fallbacks | Medium (flex) | 🟡 | M |
| QA-01 | Backend↔Flutter serialization contract test | High (flex) | 🟡 | M |
| QA-02 | Re-audit: no int/surrogate used for scoping | High | 🟡 | S |

**Sequence:** STRAT-01 → BE-01/BE-03 + FE-01 → FE-02/FE-03/FE-04 → QA. FE-01 (centralize types) must come *before* FE-02 (the sweep), or you repeat the 15-file problem.

---

## Cards

### STRAT-01 — Lock the identifier policy 🔴
**Why red:** this is FAIR-06; one owner decides it once. **Recommended policy:**
- Internal clustered PK: `int/bigint`, **never serialized**.
- External/API identifier: **slug** for salons; **Guid-as-string** for other entities.
- Flutter: **String** IDs everywhere — no int exposed anywhere.
Document it in one place; every card below follows it.

### BE-01 — Stop exposing internal `int` in API 🟡
**Priority:** Critical
**Check:** find every response that emits `t.SalonId` / an int id (`GET /api/salons`, `/api/admin/salons`, dashboard DTOs, favorites).
**Steps:** Replace the serialized `id` with the external identifier (slug for salons, guid for entities). The int stays in the DB as the PK but never crosses the API.
**Done when:** no API response or request body contains an int entity id.
**Review focus:** scoping/auth still uses `TenantId` (Guid) only — unaffected.

### BE-02 — Index the slug; single-lookup resolution 🟡
**Priority:** High (performance)
**Steps:** Confirm/add a **unique index** on salon `slug`. Ensure `/salon/{slug}/...` endpoints resolve slug→tenant in one indexed lookup, then query by tenant — no scan, no N+1.
**Done when:** slug lookup uses the index (verify the query plan); salon-page endpoints issue a bounded number of queries.

### BE-03 — Migrate `SavedSalon` to slug 🟡
**Priority:** High
**Steps:** Change `SavedSalon` to store the salon **slug** (external id) instead of `int SalonId`. Keep the denormalized `SalonName`/`LogoUrl` (apply the FAIR-11 staleness handling). This replaces the completion-report decision to key favorites on int.
**Done when:** favorites reference salons by slug across tenants; migration applies; no int salon key remains on the entity.

### BE-04 — Verify clustered-PK strategy 🔴
**Priority:** Medium (performance) · verify-first
**Why red:** a schema/perf decision. **Check:** is any high-insert table (e.g. Appointments) using a random `Guid` as its *clustered* PK? If so, random-Guid clustered keys cause page splits/fragmentation. **Recommendation (human decides):** keep `int/bigint` clustered PK + `Guid` as a non-clustered unique external key. Agent reports the current PK setup only.

### FE-01 — Centralize ID types 🟡
**Priority:** High (flexibility)
**Steps:** Introduce one source of truth for ID types in Dart — `typedef SalonId = String; typedef EntityId = String;` (or a tiny value-object module). Reference these aliases instead of raw `String`/`int`.
**Done when:** the alias module exists; the next ID change is a one-line edit, not a 15-file sweep.
**Do this before FE-02.**

### FE-02 — Sweep Flutter to String IDs 🟡
**Priority:** High · **after FE-01**
**Steps:** Convert all entity/model/repo/provider/screen IDs to the FE-01 aliases (String). **No exceptions** — `FavoriteSalon.salonId`, admin salons, dashboard models, slot/appointment/artist/service IDs all become String. Booking body sends slug/guid strings naturally.
**Done when:** no `int` id remains anywhere in Flutter; app compiles and `flutter analyze` is clean.

### FE-03 — Standardize salon sub-resource URLs 🟡
**Priority:** Medium
**Steps:** Use `/salon/{slug}/artists` and `/salon/{slug}/services` everywhere; remove the old `/api/artists/salon/{id}` and `/api/services/salon/{id}` forms.
**Done when:** one consistent URL convention for salon sub-resources; no int-keyed salon URLs remain.

### FE-04 — Centralize JSON mapping 🟡
**Priority:** Medium (flexibility)
**Steps:** Replace per-repository fallback parsing (e.g. `startsAt`/`startTime` in `appointment_repository_impl.dart`) with a single mapping layer or explicit DTO field names aligned to the backend contract. Either align the backend `SlotDto` to `startTime`/`endTime` or map once, centrally — not in each repo.
**Done when:** field-name handling lives in one place; no scattered fallback branches.

### QA-01 — Serialization contract test 🟡
**Priority:** High (flexibility)
**Steps:** Add a test that asserts each backend DTO and its Flutter model agree on field names and types (a golden-JSON or schema check). So drift fails CI instead of being silently absorbed by runtime fallbacks.
**Done when:** a deliberate field/type mismatch fails the test.

### QA-02 — Re-audit scoping 🟡
**Priority:** High
**Steps:** Re-run FAIR-07: confirm no int/surrogate key is used in any tenant filter, ownership, or auth decision after the change. Scoping is `Guid TenantId` only.
**Done when:** the audit report shows zero scoping uses of the internal int.

---

## Performance summary (what changed for the better)
- Internal `int/bigint` clustered PK kept (fast joins, no Guid fragmentation); Guid/slug as non-clustered external keys.
- Salon `slug` indexed; single-lookup slug→tenant resolution; no N+1 on salon pages.
- Favorites stay denormalized (name/logo on `SavedSalon`) so listing them needs no cross-tenant join — now keyed on slug, refreshed per FAIR-11.

## Flexibility summary
- One ID-type module (FE-01) → future type changes are one line, not a 15-file sweep.
- One JSON mapping layer (FE-04) + a contract test (QA-01) → field/type drift is caught automatically, not patched per repository.
- One URL convention (FE-03) and one identifier policy (STRAT-01) → no more int/Guid/slug flip-flopping.

---

## Never delegate
- The identifier policy decision (STRAT-01) and the clustered-PK decision (BE-04).
- Any change that lets the internal `int` become an external or scoping key.
Agent's sanctioned output on these: a report or a migration from the agreed policy — then stop.
