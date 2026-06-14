# SalonOS — System Architecture

A multi-tenant SaaS platform for beauty salons. Booking, staff and payroll
management, a salon-to-salon social layer, and a marketplace of service
packages sold by the platform owner.

This document is the single source of truth for the system design. Build from
it step by step. The AI agents in `.claude/agents/` read it too, so keep it
current as decisions change.

---

## 1. Strategy

Build Iran-first, ship a fully debugged beta, then open globally. The codebase
is global-ready from day one even while the first release is Persian and Iran
only. The expensive mistakes (hardcoded language, a single payment provider,
non-tenant-scoped data) are designed out now, not retrofitted later.

Growth goal: a large salon community. The social layer and feedback loops are
first-class, because the community is how new features get validated.

---

## 2. Actors and roles

Four roles. The whole permission model hangs off this.

| Role | Who | Sees |
| --- | --- | --- |
| Platform owner | You | Everything across all tenants, sells packages |
| Salon manager | Tenant admin | One salon's data only |
| Staff | Salon employee | Own schedule, own services, own payslip |
| Customer | End user | Own bookings, salon catalogs, ratings |

---

## 3. Core architectural decision: Modular Monolith now, microservices later

You are one person. Running a fleet of microservices alone means drowning in
deployment, networking, and observability work instead of shipping features.

Instead: one deployable application, split internally into strict modules
(bounded contexts). Each module owns its own data and exposes a service
interface. Modules never read each other's database tables directly. They talk
through interfaces and domain events.

That discipline is the whole point. When one module (say Booking) needs to
scale independently, it can be lifted out into its own service with little
rewrite, because nothing else was reaching into its internals.

Rule, enforced in every agent and review: **no cross-module database access.**

---

## 4. Tech stack

Recommended stack. The rationale matters more than the names.

| Layer | Choice | Why |
| --- | --- | --- |
| Backend | NestJS (TypeScript) | Its module system maps 1:1 to bounded contexts. Guards and interceptors make tenant isolation and RBAC clean. Easy later split. |
| Database | PostgreSQL | Row-Level Security for tenant isolation. Scales to unlimited tenants on a shared schema. |
| ORM | Prisma | Type-safe, strong migrations, the model LLMs write most reliably. |
| API | REST + OpenAPI | Simpler than GraphQL for a solo build. First-class Swagger in Nest. |
| Mobile | React Native + Expo (TypeScript) | One language across backend and app. One mental model for a solo dev. |
| Auth | Phone/OTP first, abstracted | Iran needs SMS OTP. The provider sits behind an interface so email/social slot in for global. |
| Async jobs | BullMQ + Redis | Inventory alerts, notifications, payroll runs. |
| Infra | Docker, managed Postgres, single host first | Stateless app, scale horizontally when there is a reason to. |

Backend language is the one fork worth flagging. If you prefer Python, swap
NestJS for FastAPI plus SQLAlchemy. The architecture, the multi-tenancy model,
and most skills stay identical. Only the stack-pinned files change
(`api-conventions`, `database-conventions`, `backend-builder`).

---

## 5. Bounded contexts (the module map)

Each becomes a NestJS module under `src/modules/`. Each owns its tables, its
service layer, its DTOs.

1. **identity** — accounts, sessions, OTP, the four roles, RBAC. Cross-cutting.
2. **tenancy** — salons as tenants, tenant context resolution, isolation. Cross-cutting.
3. **booking** — availability, slots, appointments. The core. Models after booking.com.
4. **staff** — employee profiles, specialties, service history.
5. **payroll** — per-employee accounting, bonuses, deductions, payslips. Financial, handle with care.
6. **catalog** — service templates the platform sells, instantiated per salon (the marketplace).
7. **inventory** — consumable materials (nail polish colors, etc.), depletion alerts.
8. **reviews** — customer ratings of staff and salons.
9. **community** — posts, comments, salon profiles, the LinkedIn-style layer. (Recruitment later.)
10. **payments** — provider-agnostic payments and payouts. Cross-cutting.
11. **notifications** — SMS, push, in-app. Cross-cutting.

The marketplace (catalog) is the most distinctive and the most coupled piece.
It touches booking (a package must be bookable), customization (color, model,
repair options), and inventory (a service consumes materials). Model a package
as a flexible **service template**: a template plus configurable attributes
plus links to inventory items. Done right, adding any new service type (hair,
skin) needs zero rewrites.

---

## 6. Multi-tenancy model

Shared schema, single database. Every tenant-owned row carries `tenant_id`.

Three layers of defense, all required:

1. **Application** — a Nest middleware resolves the tenant from the auth token
   into a request-scoped context. Every repository call is scoped by it.
2. **Database** — Postgres Row-Level Security policies on every tenant table, so
   even a bug in app code cannot leak across tenants.
3. **Tests** — every module ships a cross-tenant access test that proves tenant
   A cannot read tenant B's data.

Full rules live in `.claude/skills/multi-tenancy/`.

---

## 7. Global-ready seams

Build these open from day one even though release one is Iran only.

- **i18n and direction** — all user-facing text comes from translation
  catalogs, never hardcoded. Layout supports both RTL and LTR. Ship Persian
  first, second language drops in with no code change.
- **Payments** — never bind to one gateway. A `PaymentProvider` interface sits
  in front. Iranian gateways behind one adapter, global processors behind
  another, chosen by config. Sanctions mean Iranian and global gateways never
  overlap, so pluggability is mandatory, not optional. Rules in
  `.claude/skills/payments/`.
- **Money** — stored as integer minor units plus an explicit currency code.
  Never assume Toman. Never use floats for money.
- **Hosting** — keep the app stateless so migrating hosts (and crossing the
  sanctions boundary for the global launch) stays cheap.

---

## 8. Repository structure

```
salonos/
├── ARCHITECTURE.md          # this file
├── CLAUDE.md                # rules loaded by Claude Code every session
├── .claude/
│   ├── skills/              # reusable knowledge, loaded on demand
│   │   ├── multi-tenancy/
│   │   ├── payments/
│   │   └── ...              # (catalog below)
│   └── agents/              # subagents, the workforce
│       ├── module-architect.md
│       ├── backend-builder.md
│       └── ...              # (catalog below)
├── apps/
│   ├── api/                 # NestJS backend
│   │   └── src/modules/     # one folder per bounded context
│   └── mobile/              # Expo / React Native
├── packages/
│   └── shared/              # types, i18n catalogs, money utils shared by api + mobile
└── docs/
    └── adr/                 # architecture decision records, written by module-architect
```

---

## 9. The agent and skill system

The division of labor you asked for.

- **You** are product owner and reviewer.
- **Claude (advisor)** designs, plans, and reviews.
- **Claude Code** is the workforce that writes code, using the skills and agents below.
- **Local LLM (Ollama)** handles the cheap, high-volume, low-risk work.

### What runs where

Send to **Ollama** (local, free, fast, lower judgment): boilerplate first
drafts, DTO scaffolding, translation-string extraction, test stubs, commit
message drafts, simple classification. Anything where a wrong answer is cheap to
catch.

Send to **Claude Code** (Opus/Sonnet, high judgment): architecture, any module
touching money or tenant isolation, security review, debugging, anything where a
wrong answer is expensive. Never offload payments, payroll, or multi-tenancy
logic to the local model.

### Skills vs agents

- A **skill** is reusable knowledge (rules, conventions, procedures) that loads
  into context when relevant. Skills run in the main conversation.
- A **subagent** is a worker with its own context window, its own tool
  restrictions, and a focused job. Subagents cannot spawn other subagents.

### Skill catalog

| Skill | Purpose | Status |
| --- | --- | --- |
| multi-tenancy | Tenant isolation rules, RLS, scoping | built |
| payments | Provider-agnostic payments, money handling | built |
| i18n-localization | Translation catalogs, RTL/LTR, currency | to build |
| api-conventions | REST + OpenAPI + Nest module conventions | to build |
| database-conventions | Prisma schema and migration conventions | to build |
| testing-conventions | Test layout, the mandatory cross-tenant test | to build |
| service-template-modeling | How catalog packages, options, inventory link | to build |

### Agent catalog

| Agent | Job | Model | Tools | Status |
| --- | --- | --- | --- | --- |
| module-architect | Plan a module, write its spec/ADR before code | opus | read + write docs | built |
| backend-builder | Implement a NestJS module from the spec | sonnet | full coding | built |
| mobile-builder | Build Expo screens against the API | sonnet | full coding | to build |
| code-reviewer | Read-only review, tenancy and security focus | inherit | read only | to build |
| test-writer | Write tests, including the cross-tenant test | sonnet | coding | to build |
| db-migrator | Author and verify Prisma migrations | sonnet | coding | to build |

Remaining skills and agents get built as you reach the module that needs them.
That is the step-by-step path, not a big bang.

---

## 10. Build roadmap

Each phase is shippable and testable before the next starts.

**Phase 0 — Foundations.** Repo, CLAUDE.md, the skills and agents above. NestJS
app skeleton, Prisma, Postgres with RLS turned on. `identity` and `tenancy`
modules. OTP auth. The cross-tenant test passes. Nothing else works yet, but the
spine is correct.

**Phase 1 — Booking MVP.** `booking`, `staff`, basic salon profile. A manager
creates staff and services, a customer books a slot. This is the beating heart.
Get it solid.

**Phase 2 — Money and people.** `payroll` (per-employee accounting, bonuses,
deductions), `reviews` (customer ratings). `payments` wired to one Iranian
gateway through the abstraction.

**Phase 3 — Marketplace.** `catalog` service templates, the platform-owner sell
flow, per-salon instantiation, customization options, and `inventory` with
depletion alerts. The distinctive layer.

**Phase 4 — Community.** `community` posts, comments, salon profiles, the
competitive feed. Lightweight version can land earlier if it helps validate.

**Phase 5 — Global.** Second language, global payment adapter, international
hosting. Because the seams were open from Phase 0, this is configuration and
adapters, not a rewrite.

Future, slotting in without structural change: recruitment (extends community),
analytics, and whatever the community asks for.
