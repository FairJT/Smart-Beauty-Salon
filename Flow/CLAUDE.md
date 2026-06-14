# SalonOS — Project rules for Claude Code

Read `ARCHITECTURE.md` for the full design. This file is the short, always-loaded
rulebook. Follow it in every session and every subagent.

## What we are building

A multi-tenant SaaS for beauty salons: booking, staff and payroll, a salon
social layer, and a marketplace of service packages. Iran-first, then global.
One developer, so the architecture must stay simple to operate.

## Stack

NestJS + TypeScript backend. PostgreSQL + Prisma. REST + OpenAPI. React Native +
Expo mobile. BullMQ + Redis for jobs. Phone/OTP auth behind a provider interface.

## Non-negotiable rules

1. **Modular monolith.** One app, strict modules under `src/modules/`. A module
   never reads another module's tables. Modules talk through service interfaces
   and domain events only. This is what keeps a later microservice split cheap.

2. **Multi-tenancy.** Every tenant-owned row has `tenant_id`. Every query is
   scoped by the request tenant context. Postgres RLS is on for every tenant
   table as a second line of defense. See `.claude/skills/multi-tenancy/`.

3. **Money.** Integer minor units plus an explicit currency code. Never floats.
   Never assume Toman. See `.claude/skills/payments/`.

4. **Payments.** Never call a gateway SDK from domain code. Go through the
   `PaymentProvider` interface. Iranian and global providers are separate
   adapters chosen by config.

5. **i18n.** No hardcoded user-facing strings. Everything from translation
   catalogs. Layout supports RTL and LTR. Ship Persian first.

6. **Tests.** Every module ships a test proving tenant A cannot access tenant B.
   No module is "done" without it.

## The LLM split

Offload to the local Ollama model: boilerplate drafts, DTO scaffolding, string
extraction, test stubs, commit messages, simple classification.

Keep on Claude (me): architecture, anything touching money, payroll, payments,
or tenant isolation, security review, and debugging. Never let the local model
own correctness-critical logic.

## Workflow

Build one module at a time, following the roadmap in `ARCHITECTURE.md`.

For each module: run `module-architect` first to produce a spec/ADR under
`docs/adr/`, then `backend-builder` to implement it, then review. Do not write a
module's code before its spec exists.

## Conventions

- Commit small and often. One logical change per commit.
- New decisions that change the design go into `ARCHITECTURE.md` and a short ADR.
- English for all code, identifiers, and these docs. Persian only for
  user-facing strings, which live in translation catalogs.
