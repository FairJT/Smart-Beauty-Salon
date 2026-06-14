# SalonOS — START HERE

فایل ورودی پروژه. هر فاز را کامل کن و تأییدیه بگیر قبل از اینکه به فاز بعد بروی.

---

## پیش از شروع (یک‌بار، در ترمینال)

```powershell
$env:CLAUDE_CODE_TASK_LIST_ID = "salonos"
```

---

## فاز ۱ — خواندن و تأیید

همه‌ی مستندات پروژه را بخوان و قوانین را تأیید کن.

```
Read these files in order and confirm understanding after each:
1. CLAUDE.md               — project rules, always active
2. ARCHITECTURE.md         — system design and module map
3. docs/PLAN.md            — full task list with dependencies
4. docs/build-runbook.md   — step-by-step acceptance criteria
5. .claude/README.md       — skills and agents index

After reading all five, summarize:
- the three non-negotiable rules from CLAUDE.md
- the nine bounded contexts from ARCHITECTURE.md
- which skills exist now and which must be created first
```

**تمام وقتی:** Claude Code سه قانون، نه ماژول، و وضعیت اسکیل‌ها را درست بازگو کند.

---

## فاز ۲ — ساخت زیرساخت (اسکیل‌ها و ایجنت‌های باقی‌مانده)

فایل‌های `.claude/` که هنوز وجود ندارند را بساز.
ترتیب مهم است: اسکیل‌های پایه اول، بعد ایجنت‌هایی که به آنها وابسته‌اند.

```
Build the missing .claude/ files in this exact order.
For each, follow the format and frontmatter conventions of the existing
skills and agents in .claude/README.md.

Skills to create:
1. .claude/skills/database-conventions/SKILL.md
2. .claude/skills/api-conventions/SKILL.md
3. .claude/skills/testing-conventions/SKILL.md
4. .claude/skills/i18n-localization/SKILL.md
5. .claude/skills/service-template-modeling/SKILL.md

Agents to create (after their required skills exist):
6. .claude/agents/db-migrator.md        (needs: database-conventions)
7. .claude/agents/test-writer.md        (needs: testing-conventions)
8. .claude/agents/code-reviewer.md      (needs: multi-tenancy, payments)
9. .claude/agents/mobile-builder.md     (needs: api-conventions, i18n-localization)

After creating all nine files, run a quick check:
list every file now present under .claude/ and confirm all nine are there.
```

**تمام وقتی:** هر ۹ فایل وجود داشته باشند. سشن Claude Code را ری‌استارت کن تا بارگذاری شوند.

---

## فاز ۳ — اسکلت پروژه

ریپو، دیتابیس، Prisma، و پلامبینگ کانتکست مستاجر را بساز.

```
Scaffold the project as defined in ARCHITECTURE.md section 8.

Step 1 — Monorepo structure:
  apps/api     NestJS + TypeScript
  apps/mobile  Expo placeholder
  packages/shared  money utility, shared types, i18n catalog shell

Step 2 — Ollama helper scripts:
  scripts/ask-local.ps1  (PowerShell)
  scripts/ask-local.sh   (bash)
  Each takes a prompt arg and calls: ollama run qwen2.5-coder

Step 3 — Docker Compose:
  PostgreSQL and Redis with health checks
  .env.example with all connection variables

Step 4 — Prisma:
  Connect to Postgres, Account model (global, no tenantId), first migration

Step 5 — TenantContext plumbing:
  Request-scoped TenantContext
  Middleware that resolves tenant from JWT into context
  Sets app.current_tenant Postgres session variable per request
  Apply the multi-tenancy skill throughout

After each step confirm it works before starting the next.
```

**تمام وقتی:** `GET /health` جواب ok بدهد، `docker compose up -d` بالا بیاید، migration اجرا شده باشد، TenantContext یک تست واحد داشته باشد.

---

## فاز ۴ — ماژول‌های پایه (tenancy و identity)

دو ماژول بنیادی را طراحی و بساز. **قبل از کد زدن، ADR را بنویس و بخوان.**

```
Step 1 — Design:
Use the module-architect agent to design the tenancy module.
Write the ADR to docs/adr/0001-tenancy.md
Wait for review before proceeding.

Step 2 — Design:
Use the module-architect agent to design the identity module
(phone/OTP auth, four roles, RBAC guards, OTP provider interface).
Write the ADR to docs/adr/0002-identity.md
Wait for review before proceeding.

Step 3 — Implement:
Use the backend-builder agent to implement the tenancy module from its ADR.
Apply skills: multi-tenancy, database-conventions, api-conventions.

Step 4 — Implement:
Use the backend-builder agent to implement the identity module from its ADR.
Apply skills: multi-tenancy, api-conventions.

Step 5 — RLS:
Use the db-migrator agent to add the Row-Level Security migration:
enable RLS on all tenant tables, add tenant_isolation policy keyed on
app.current_tenant, add the platform-owner bypass in one named place.

Step 6 — Gate test:
Use the test-writer agent to write and run the mandatory cross-tenant
isolation test: tenant A cannot read or mutate tenant B's data by id.
This test must be green before Phase 5 starts.
```

**تمام وقتی:** تست cross-tenant سبز باشد. این دروازه‌ی فاز ۴ است.

---

## فاز ۵ — ماژول‌به‌ماژول (حلقه‌ی تکرارشونده)

از اینجا به بعد هر ماژول را با همین حلقه بساز.
ترتیب ماژول‌ها از `docs/PLAN.md` و نقشه‌راه `ARCHITECTURE.md` بخوان.

```
For each module in roadmap order:

1. DESIGN
   Use module-architect agent.
   Write ADR to docs/adr/NNNN-<module>.md
   Stop and wait for human review of the ADR.

2. BUILD
   Use backend-builder agent (read the ADR first).
   Apply relevant skills (check .claude/README.md for which ones).

3. REVIEW
   Use code-reviewer agent.
   Fix any violations it finds.

4. TEST
   Use test-writer agent.
   Cross-tenant test must pass for every tenant-owned module.

5. COMMIT
   Small commit. Then /clear before the next module.
```

Module order: staff → booking → payroll → reviews → payments → catalog → inventory → community → mobile screens → global.

**تمام وقتی:** همان‌طور که PLAN.md می‌گوید هر ماژول را gate کن.
