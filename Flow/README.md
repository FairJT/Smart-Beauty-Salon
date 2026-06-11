# .claude — Skills and Agents Index

Everything under this folder is loaded by Claude Code on demand.
Read this file to know what exists, when to use it, and what is still missing.

---

## Skills

Skills are reusable rules that load into context when relevant.
The `description` frontmatter in each SKILL.md determines when Claude Code
preloads it. Use the `skills:` key in an agent's frontmatter to force-preload.

| Order | Skill | File | Status | Use when |
| --- | --- | --- | --- | --- |
| 1 | multi-tenancy | `skills/multi-tenancy/SKILL.md` | ✅ built | Any model, query, or migration on tenant-owned data |
| 2 | payments | `skills/payments/SKILL.md` | ✅ built | Any money value, payment flow, or gateway code |
| 3 | database-conventions | `skills/database-conventions/SKILL.md` | 🔲 Phase 2 | Any Prisma schema, migration, or index decision |
| 4 | api-conventions | `skills/api-conventions/SKILL.md` | 🔲 Phase 2 | Any NestJS controller, DTO, guard, or OpenAPI decorator |
| 5 | testing-conventions | `skills/testing-conventions/SKILL.md` | 🔲 Phase 2 | Any test file, cross-tenant test, or test structure decision |
| 6 | i18n-localization | `skills/i18n-localization/SKILL.md` | 🔲 Phase 2 | Any user-facing string, RTL/LTR layout, or currency display |
| 7 | service-template-modeling | `skills/service-template-modeling/SKILL.md` | 🔲 Phase 3 | Catalog packages, option sets, inventory links |

**Rule:** skills 1 and 2 apply to nearly everything. When in doubt, preload them.

---

## Agents

Agents are subagents with their own focused context and tool restrictions.
They are invoked by name, not triggered automatically.

| Order | Agent | File | Model | Status | Invoke when |
| --- | --- | --- | --- | --- | --- |
| 1 | module-architect | `agents/module-architect.md` | opus | ✅ built | Designing any new module before code is written |
| 2 | backend-builder | `agents/backend-builder.md` | sonnet | ✅ built | Implementing a NestJS module from a finished ADR |
| 3 | db-migrator | `agents/db-migrator.md` | sonnet | 🔲 Phase 2 | Writing or reviewing any Prisma migration |
| 4 | test-writer | `agents/test-writer.md` | sonnet | 🔲 Phase 2 | Writing tests, especially the cross-tenant gate test |
| 5 | code-reviewer | `agents/code-reviewer.md` | inherit | 🔲 Phase 2 | Read-only review after a module is built |
| 6 | mobile-builder | `agents/mobile-builder.md` | sonnet | 🔲 Phase 4 | Building Expo/React Native screens |

---

## Standard module workflow

```
module-architect  →  (human reviews ADR)  →  backend-builder
→  db-migrator (if new migration)  →  test-writer  →  code-reviewer
→  commit  →  /clear  →  next module
```

Never skip the architect step. Never build a module without a reviewed ADR.

---

## LLM routing

| Task | Use |
| --- | --- |
| Architecture, ADR, complex debug | Claude (Opus via module-architect) |
| Module implementation, tests | Claude (Sonnet via backend-builder / test-writer) |
| Review, security, isolation check | Claude (code-reviewer, inherit model) |
| Boilerplate drafts, DTO stubs, commit messages | Ollama via scripts/ask-local.ps1 |
| Money, payroll, payments, tenancy logic | Always Claude — never Ollama |
