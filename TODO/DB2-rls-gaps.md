# DB2 — Put the last 3 tenant tables under RLS 🟡 (review after)

`Memberships`, `ArtistProfiles`, `SalonManagerProfiles` have `TenantId` but aren't in the RLS policy.

**File:** `src/SalonOS.Infrastructure/Migrations/AddRLS.sql`

**Find (exact):**
```sql
    WITH (STATE = ON, SCHEMABINDING = ON);
```

**Replace with:**
```sql
    ,
    ADD FILTER PREDICATE [Security].[fn_tenant]([TenantId]) ON [dbo].[ArtistProfiles],
    ADD BLOCK  PREDICATE [Security].[fn_tenant]([TenantId]) ON [dbo].[ArtistProfiles]  AFTER INSERT,
    ADD FILTER PREDICATE [Security].[fn_tenant]([TenantId]) ON [dbo].[Memberships],
    ADD BLOCK  PREDICATE [Security].[fn_tenant]([TenantId]) ON [dbo].[Memberships]  AFTER INSERT,
    ADD FILTER PREDICATE [Security].[fn_tenant]([TenantId]) ON [dbo].[SalonManagerProfiles],
    ADD BLOCK  PREDICATE [Security].[fn_tenant]([TenantId]) ON [dbo].[SalonManagerProfiles]  AFTER INSERT

    WITH (STATE = ON, SCHEMABINDING = ON);
```
(Leading `,` closes the previous last predicate; the final line has no comma before `WITH`.)

**⚠️ Verify FIRST** that each table actually has a `TenantId` column:
```powershell
Select-String -Path src\Modules\Identity\Domain\*.cs -Pattern "class (ArtistProfile|Membership|SalonManagerProfile)|public Guid TenantId"
```
Only keep a table in the list if it has `TenantId`. (`ClientProfiles`, `JobSeekerProfiles`, `SavedSalons`
are GLOBAL — they must NOT be added.)

**Done when:** the policy lists the 3 tables and the RLS script still parses on startup.
**After deploy:** confirm an authenticated manager can still read staff/memberships (not empty). If empty,
a query is missing tenant context — report to Claude.
