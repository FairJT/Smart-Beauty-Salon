# Agent Tasks — DB optimization

From `DB-Optimization-Report.md`. Small, ordered. Flags: 🟡 review after · 🟢 safe.

> Rule: do ONLY what each step says. If a "Find" isn't found exactly, STOP and report.

| # | File | What | Flag |
|---|------|------|------|
| DB1 | `DB1-indexes-and-lengths.md` | FK indexes + key string max-lengths (+ migration) | 🟡 |
| DB2 | `DB2-rls-gaps.md` | Put `Memberships`/`ArtistProfiles`/`SalonManagerProfiles` under RLS | 🟡 |
| DB3 | `DB3-inventory-decimals.md` | Decimal precision on inventory columns (optional) | 🟢 |

After: `dotnet build SalonOS.slnx`, then on a test DB apply migrations and confirm they run.

## Deferred (after SuperAdmin batch lands)
The global platform tables (BlogPost, SalonPlacement, Homepage*, SalonJoinRequest) will need their own
indexes/lengths — a small follow-up batch once those entities exist.
