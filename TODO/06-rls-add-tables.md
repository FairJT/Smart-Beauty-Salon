# Task 06 — Add ArtistSchedules + Leaves to the RLS policy 🟢

Make ONLY this change. Do not edit anything else.
Watch the commas: the OLD last line gains a comma; the NEW last line has none.

**File:** `src/SalonOS.Infrastructure/Migrations/AddRLS.sql`

**Find (exact):**
```sql
    ADD FILTER PREDICATE [Security].[fn_tenant]([TenantId]) ON [dbo].[SalonPackageLicenses],
    ADD BLOCK  PREDICATE [Security].[fn_tenant]([TenantId]) ON [dbo].[SalonPackageLicenses]  AFTER INSERT

    WITH (STATE = ON, SCHEMABINDING = ON);
```

**Replace with:**
```sql
    ADD FILTER PREDICATE [Security].[fn_tenant]([TenantId]) ON [dbo].[SalonPackageLicenses],
    ADD BLOCK  PREDICATE [Security].[fn_tenant]([TenantId]) ON [dbo].[SalonPackageLicenses]  AFTER INSERT,

    ADD FILTER PREDICATE [Security].[fn_tenant]([TenantId]) ON [dbo].[ArtistSchedules],
    ADD BLOCK  PREDICATE [Security].[fn_tenant]([TenantId]) ON [dbo].[ArtistSchedules]  AFTER INSERT,

    ADD FILTER PREDICATE [Security].[fn_tenant]([TenantId]) ON [dbo].[Leaves],
    ADD BLOCK  PREDICATE [Security].[fn_tenant]([TenantId]) ON [dbo].[Leaves]  AFTER INSERT

    WITH (STATE = ON, SCHEMABINDING = ON);
```

**Done when:** the policy lists `ArtistSchedules` and `Leaves`, and only the final
`... [Leaves] ... AFTER INSERT` line has NO trailing comma.

**Verify (PowerShell):**
```powershell
Select-String -Path src\SalonOS.Infrastructure\Migrations\AddRLS.sql -Pattern "ArtistSchedules|Leaves"
```
Expect 4 hits (2 each).
