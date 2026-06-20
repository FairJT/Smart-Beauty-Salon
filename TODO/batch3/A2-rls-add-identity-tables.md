# Task A2 — Put the remaining tenant tables under RLS 🟡 (review after)

`rls-gaps.md` found `ArtistProfiles`, `Memberships`, `SalonManagerProfiles` carry `TenantId`
but aren't under RLS. Add them. (These are the real table names; the audit guessed.)

**File:** `src/SalonOS.Infrastructure/Migrations/AddRLS.sql`

**Find (exact):**
```sql
    ADD FILTER PREDICATE [Security].[fn_tenant]([TenantId]) ON [dbo].[Leaves],
    ADD BLOCK  PREDICATE [Security].[fn_tenant]([TenantId]) ON [dbo].[Leaves]  AFTER INSERT

    WITH (STATE = ON, SCHEMABINDING = ON);
```

**Replace with:**
```sql
    ADD FILTER PREDICATE [Security].[fn_tenant]([TenantId]) ON [dbo].[Leaves],
    ADD BLOCK  PREDICATE [Security].[fn_tenant]([TenantId]) ON [dbo].[Leaves]  AFTER INSERT,

    ADD FILTER PREDICATE [Security].[fn_tenant]([TenantId]) ON [dbo].[ArtistProfiles],
    ADD BLOCK  PREDICATE [Security].[fn_tenant]([TenantId]) ON [dbo].[ArtistProfiles]  AFTER INSERT,

    ADD FILTER PREDICATE [Security].[fn_tenant]([TenantId]) ON [dbo].[Memberships],
    ADD BLOCK  PREDICATE [Security].[fn_tenant]([TenantId]) ON [dbo].[Memberships]  AFTER INSERT,

    ADD FILTER PREDICATE [Security].[fn_tenant]([TenantId]) ON [dbo].[SalonManagerProfiles],
    ADD BLOCK  PREDICATE [Security].[fn_tenant]([TenantId]) ON [dbo].[SalonManagerProfiles]  AFTER INSERT

    WITH (STATE = ON, SCHEMABINDING = ON);
```
(Note: a comma was added after the old `Leaves` line; the new last line `SalonManagerProfiles ... AFTER INSERT` has NO trailing comma.)

**Done when:** the policy lists all three new tables.

**⚠️ Human review — do NOT add these blindly:** confirm each table really has a `TenantId` column
(`ClientProfiles`, `JobSeekerProfiles`, `SavedSalons` are GLOBAL — they must stay OUT of RLS).
Verify:
```powershell
Select-String -Path src\Modules\Identity\Domain\*.cs -Pattern "class (ArtistProfile|Membership|SalonManagerProfile)|TenantId"
```
Only add a table if it has `TenantId`. After deploy, an authenticated request to e.g. the staff
list must still return rows (if it returns empty, a table was wrongly added or a query lacks tenant context — report back).
