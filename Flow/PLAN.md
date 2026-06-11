# SalonOS — Master Build Plan

This is the single entry point for Claude Code. Read every file listed in the
File Inventory below before executing any task. The task list drives the entire
project from repo scaffold to global launch. Every task references which agent
runs it, which skills to preload, and what the acceptance criterion is.

---

## How to start (operator, run once in terminal)

```powershell
# PowerShell — set before opening Claude Code
$env:CLAUDE_CODE_TASK_LIST_ID = "salonos"
```

Then paste this prompt into Claude Code:

```
Read every file listed under "File Inventory" in docs/PLAN.md, in the order
shown. Then convert the full task list in docs/PLAN.md into Claude Code tasks
using TaskCreate, respecting all dependency chains. Mark tasks with no
dependencies as ready. After creating all tasks, run TaskList to confirm the
dependency graph, then begin executing from the first ready task.
```

---

## File Inventory

Read these files in this exact order before doing anything else.

| File | Role | Read when |
| --- | --- | --- |
| `CLAUDE.md` | Always-loaded project rules. Non-negotiable. | First, every session |
| `ARCHITECTURE.md` | Full system design, module map, roadmap, agent+skill catalog | Second, every session |
| `docs/build-runbook.md` | Step-by-step operator context and acceptance criteria per step | Reference during Phase 0 |
| `docs/PLAN.md` | This file — master task list and entry point | Always |
| `.claude/skills/multi-tenancy/SKILL.md` | Tenant isolation rules, RLS, scoping — apply to every tenant-owned model | Before any data model work |
| `.claude/skills/payments/SKILL.md` | Money representation, PaymentProvider interface, idempotency | Before any payment or money work |
| `.claude/agents/module-architect.md` | Instructions for the planning subagent | When running module-architect |
| `.claude/agents/backend-builder.md` | Instructions for the implementation subagent | When running backend-builder |

---

## Rules that override everything

These come from `CLAUDE.md`. No task, no matter how small, may break them.

1. No cross-module database access. Modules talk through interfaces and events.
2. Every tenant-owned row has `tenantId`. Every query is scoped by TenantContext.
3. Money is integer minor units + explicit currency. Never floats.
4. Payments go through `PaymentProvider` interface. Never call a gateway SDK directly.
5. No hardcoded user-facing strings. Everything from i18n catalogs.
6. Every module ships a cross-tenant isolation test. No exceptions.

LLM routing: keep architecture, money, payroll, payments, and isolation on
Claude. Offload to Ollama (scripts/ask-local.ps1): boilerplate drafts, DTO
scaffolding, string extraction, test stubs.

---

## Task List

### Phase 0-A — Infrastructure: missing skills and agents

These tasks build the remaining `.claude/` md files cataloged in
`ARCHITECTURE.md` section 9 that were not yet created. They must exist before
any module that needs them.

| ID | Title | Agent | Skills | Depends on | Done when |
| --- | --- | --- | --- | --- | --- |
| P0A-T01 | Create skill: `i18n-localization` — translation catalogs, RTL/LTR, currency display, no hardcoded strings rule with examples | *(direct)* | — | — | `.claude/skills/i18n-localization/SKILL.md` exists and covers the rules |
| P0A-T02 | Create skill: `api-conventions` — NestJS REST module layout, OpenAPI decorators, DTO validation, guard pattern, response shape | *(direct)* | — | — | `.claude/skills/api-conventions/SKILL.md` exists |
| P0A-T03 | Create skill: `database-conventions` — Prisma schema rules, migration naming, index policy, nullable policy, the tenant_id index requirement | *(direct)* | multi-tenancy | — | `.claude/skills/database-conventions/SKILL.md` exists |
| P0A-T04 | Create skill: `testing-conventions` — test file layout, the mandatory cross-tenant test template, unit vs integration split, naming | *(direct)* | multi-tenancy | — | `.claude/skills/testing-conventions/SKILL.md` exists |
| P0A-T05 | Create skill: `service-template-modeling` — how catalog packages, configurable option sets, and inventory items link; the flexible attribute model | *(direct)* | multi-tenancy | — | `.claude/skills/service-template-modeling/SKILL.md` exists |
| P0A-T06 | Create agent: `mobile-builder` — Expo/React Native screens from API contracts; reads api-conventions and i18n skills; Sonnet model | *(direct)* | — | P0A-T01, P0A-T02 | `.claude/agents/mobile-builder.md` exists |
| P0A-T07 | Create agent: `code-reviewer` — read-only review focused on tenancy leaks, money handling, cross-module db access; inherit model | *(direct)* | multi-tenancy, payments | P0A-T03, P0A-T04 | `.claude/agents/code-reviewer.md` exists |
| P0A-T08 | Create agent: `test-writer` — writes unit and integration tests including the cross-tenant template; Sonnet model | *(direct)* | testing-conventions | P0A-T04 | `.claude/agents/test-writer.md` exists |
| P0A-T09 | Create agent: `db-migrator` — authors and verifies Prisma migrations, checks for missing tenant_id and missing indexes; Sonnet model | *(direct)* | database-conventions, multi-tenancy | P0A-T03 | `.claude/agents/db-migrator.md` exists |

---

### Phase 0-B — Repo and environment scaffold

| ID | Title | Agent | Skills | Depends on | Done when |
| --- | --- | --- | --- | --- | --- |
| P0B-T01 | Create Ollama helper scripts: `scripts/ask-local.ps1` and `scripts/ask-local.sh`, each accepting a prompt arg and calling `ollama run qwen2.5-coder` | *(direct)* | — | — | Both scripts run and return output |
| P0B-T02 | Scaffold monorepo: `apps/api` (NestJS + TypeScript), `apps/mobile` (Expo placeholder), `packages/shared` (money utility, i18n catalog structure, shared types) | *(direct)* | api-conventions | P0A-T01, P0A-T02 | `apps/api` starts, `/health` returns ok |
| P0B-T03 | Add `docker-compose.yml`: PostgreSQL and Redis with health checks, `.env.example` with all connection vars | *(direct)* | — | P0B-T02 | `docker compose up -d` succeeds |
| P0B-T04 | Add Prisma to `apps/api`: connect to Postgres, `Account` model (global, no tenantId), first migration runs | db-migrator | database-conventions | P0B-T03 | Migration applies, `Account` table exists |
| P0B-T05 | Implement TenantContext plumbing: request-scoped `TenantContext`, Nest middleware resolves tenant from JWT, sets `app.current_tenant` Postgres session variable | *(direct)* | multi-tenancy | P0B-T04 | Unit test proves context is set from token |

---

### Phase 0-C — Core modules: tenancy and identity

| ID | Title | Agent | Skills | Depends on | Done when |
| --- | --- | --- | --- | --- | --- |
| P0C-T01 | Design tenancy module → write ADR to `docs/adr/0001-tenancy.md` | module-architect | multi-tenancy | P0B-T05 | ADR file exists and reviewed |
| P0C-T02 | Design identity module (OTP auth, 4 roles, RBAC guards, OTP provider interface) → write ADR to `docs/adr/0002-identity.md` | module-architect | multi-tenancy | P0B-T05 | ADR file exists and reviewed |
| P0C-T03 | Implement tenancy module from ADR | backend-builder | multi-tenancy, database-conventions, api-conventions | P0C-T01 | Module compiles, tenant CRUD works |
| P0C-T04 | Implement identity module from ADR | backend-builder | multi-tenancy, api-conventions | P0C-T02, P0C-T03 | OTP login flow works, roles enforced |
| P0C-T05 | Add RLS migration: enable Row-Level Security on every tenant table, add `tenant_isolation` policy keyed on `app.current_tenant`, add platform-owner bypass in one named place | db-migrator | multi-tenancy, database-conventions | P0C-T03, P0C-T04 | RLS active on tenant tables |
| P0C-T06 | Write and pass mandatory cross-tenant isolation test: tenant A cannot read or mutate tenant B's data by id | test-writer | testing-conventions, multi-tenancy | P0C-T05 | Test green — Phase 0 gate |

---

### Phase 1 — Booking MVP

| ID | Title | Agent | Skills | Depends on | Done when |
| --- | --- | --- | --- | --- | --- |
| P1-T01 | Design staff module (employee profiles, specialties, service history) → ADR `0003-staff.md` | module-architect | multi-tenancy | P0C-T06 | ADR reviewed |
| P1-T02 | Design booking module (availability slots, appointments, calendar) → ADR `0004-booking.md` | module-architect | multi-tenancy | P0C-T06 | ADR reviewed |
| P1-T03 | Implement staff module | backend-builder | multi-tenancy, api-conventions, database-conventions | P1-T01 | Staff CRUD, cross-tenant test passes |
| P1-T04 | Implement booking module | backend-builder | multi-tenancy, api-conventions, database-conventions | P1-T02, P1-T03 | Slot creation and booking works |
| P1-T05 | Salon profile endpoints: public name, contact, working hours, active services | backend-builder | multi-tenancy, api-conventions | P1-T04 | Profile readable |
| P1-T06 | Integration test: manager creates staff and service, customer books a slot — end-to-end green | test-writer | testing-conventions | P1-T04, P1-T05 | E2E test passes — Phase 1 gate |

---

### Phase 2 — Money and People

| ID | Title | Agent | Skills | Depends on | Done when |
| --- | --- | --- | --- | --- | --- |
| P2-T01 | Design payroll module (per-employee accounting, bonuses, deductions, payslips) → ADR `0005-payroll.md` | module-architect | multi-tenancy, payments | P1-T06 | ADR reviewed |
| P2-T02 | Design reviews module (customer rating of staff and salon, score aggregation) → ADR `0006-reviews.md` | module-architect | multi-tenancy | P1-T06 | ADR reviewed |
| P2-T03 | Design payments module (PaymentProvider interface, Iranian gateway adapter) → ADR `0007-payments.md` | module-architect | payments, multi-tenancy | P1-T06 | ADR reviewed |
| P2-T04 | Implement payroll module — correctness-critical, keep on Sonnet/Opus, never offload to Ollama | backend-builder | payments, multi-tenancy, database-conventions | P2-T01 | Payslip generated, money in integer minor units |
| P2-T05 | Implement reviews module | backend-builder | multi-tenancy, api-conventions | P2-T02 | Rating submitted and aggregated |
| P2-T06 | Implement payments module with PaymentProvider interface and first Iranian gateway adapter | backend-builder | payments, multi-tenancy | P2-T03 | Payment session created and verified via adapter |
| P2-T07 | Wire webhook: verify gateway signature, idempotent handler, reconcile payment state | backend-builder | payments | P2-T06 | Webhook handler rejects invalid signatures |
| P2-T08 | Integration test: payroll calculation, deduction applied, payslip correct | test-writer | testing-conventions, payments | P2-T04 | Test green — Phase 2 gate |

---

### Phase 3 — Marketplace

| ID | Title | Agent | Skills | Depends on | Done when |
| --- | --- | --- | --- | --- | --- |
| P3-T01 | Design catalog module (platform owner sells service templates, salon instantiates with options) → ADR `0008-catalog.md` | module-architect | service-template-modeling, multi-tenancy | P2-T08 | ADR reviewed |
| P3-T02 | Design inventory module (consumable materials, stock levels, depletion alerts) → ADR `0009-inventory.md` | module-architect | service-template-modeling, multi-tenancy | P2-T08 | ADR reviewed |
| P3-T03 | Implement catalog module: platform-owner creates template, salon purchases and activates | backend-builder | service-template-modeling, multi-tenancy, payments, api-conventions | P3-T01, P3-T02 | Template purchasable, activates on salon profile |
| P3-T04 | Implement inventory module: stock tracking per salon | backend-builder | multi-tenancy, database-conventions | P3-T02 | Stock readable and decrementable |
| P3-T05 | Per-salon catalog customization: option sets (color, model, repair) on booking surface | backend-builder | service-template-modeling, api-conventions | P3-T03 | Customer can select options at booking time |
| P3-T06 | Inventory depletion alerts: when stock falls below threshold, notify salon manager | backend-builder | multi-tenancy | P3-T04 | Notification triggered on low stock |
| P3-T07 | Review catalog + inventory: code-reviewer checks cross-module access and money handling | code-reviewer | multi-tenancy, payments, service-template-modeling | P3-T05, P3-T06 | Review passed, no violations — Phase 3 gate |

---

### Phase 4 — Community

| ID | Title | Agent | Skills | Depends on | Done when |
| --- | --- | --- | --- | --- | --- |
| P4-T01 | Design community module (posts, comments, salon public profiles, competitive feed) → ADR `0010-community.md` | module-architect | multi-tenancy | P1-T06 | ADR reviewed |
| P4-T02 | Implement community module: posts, comments, salon feed | backend-builder | multi-tenancy, api-conventions | P4-T01 | Salon can post, others can comment |
| P4-T03 | Competitive feed: engagement-based ranking visible to all salons | backend-builder | multi-tenancy | P4-T02 | Feed returns salons ranked by engagement |
| P4-T04 | Mobile screens — booking flow: slot picker, booking confirmation, my appointments | mobile-builder | api-conventions, i18n-localization | P1-T06 | Screens render and call API |
| P4-T05 | Mobile screens — staff and catalog: salon profile, service menu with options | mobile-builder | api-conventions, i18n-localization | P3-T05 | Customer can browse and select services |
| P4-T06 | Mobile screens — community feed | mobile-builder | i18n-localization | P4-T03 | Feed visible in app — Phase 4 gate |

---

### Phase 5 — Global

| ID | Title | Agent | Skills | Depends on | Done when |
| --- | --- | --- | --- | --- | --- |
| P5-T01 | Add second language to i18n catalogs, verify RTL/LTR layout switch with no hardcoded strings remaining | *(direct)* | i18n-localization | P4-T06 | App renders correctly in both languages |
| P5-T02 | Implement global payment adapter (Stripe or equivalent) behind existing `PaymentProvider` interface, zero changes to domain code | backend-builder | payments | P2-T06 | Global adapter passes same tests as Iranian adapter |
| P5-T03 | Infrastructure: migrate stateless app to globally-accessible host, update env config, smoke-test all modules | *(direct)* | — | P5-T01, P5-T02 | All Phase 0-4 integration tests green on new host — project complete |
