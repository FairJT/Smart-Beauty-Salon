# MZ — Put new tables under RLS (defense in depth) 🟡 (do LAST, review after)

Do this AFTER M2–M7 entities exist. The new entities are ALREADY tenant-scoped automatically by
`AppDbContext` (Layer 2), so this only adds the DB-level RLS backstop (Layer 3) for consistency.

**File:** `src/SalonOS.Infrastructure/Migrations/AddRLS.sql`

**Find (exact):**
```sql
    WITH (STATE = ON, SCHEMABINDING = ON);
```

**Replace with:**
```sql
    ,
    ADD FILTER PREDICATE [Security].[fn_tenant]([TenantId]) ON [dbo].[SalonAmenities],
    ADD BLOCK  PREDICATE [Security].[fn_tenant]([TenantId]) ON [dbo].[SalonAmenities]  AFTER INSERT,
    ADD FILTER PREDICATE [Security].[fn_tenant]([TenantId]) ON [dbo].[SalonNotices],
    ADD BLOCK  PREDICATE [Security].[fn_tenant]([TenantId]) ON [dbo].[SalonNotices]  AFTER INSERT,
    ADD FILTER PREDICATE [Security].[fn_tenant]([TenantId]) ON [dbo].[WorkingHours],
    ADD BLOCK  PREDICATE [Security].[fn_tenant]([TenantId]) ON [dbo].[WorkingHours]  AFTER INSERT,
    ADD FILTER PREDICATE [Security].[fn_tenant]([TenantId]) ON [dbo].[SalonClosures],
    ADD BLOCK  PREDICATE [Security].[fn_tenant]([TenantId]) ON [dbo].[SalonClosures]  AFTER INSERT,
    ADD FILTER PREDICATE [Security].[fn_tenant]([TenantId]) ON [dbo].[StaffServiceContracts],
    ADD BLOCK  PREDICATE [Security].[fn_tenant]([TenantId]) ON [dbo].[StaffServiceContracts]  AFTER INSERT,
    ADD FILTER PREDICATE [Security].[fn_tenant]([TenantId]) ON [dbo].[FinancialTransactions],
    ADD BLOCK  PREDICATE [Security].[fn_tenant]([TenantId]) ON [dbo].[FinancialTransactions]  AFTER INSERT,
    ADD FILTER PREDICATE [Security].[fn_tenant]([TenantId]) ON [dbo].[Discounts],
    ADD BLOCK  PREDICATE [Security].[fn_tenant]([TenantId]) ON [dbo].[Discounts]  AFTER INSERT,
    ADD FILTER PREDICATE [Security].[fn_tenant]([TenantId]) ON [dbo].[JobPostings],
    ADD BLOCK  PREDICATE [Security].[fn_tenant]([TenantId]) ON [dbo].[JobPostings]  AFTER INSERT,
    ADD FILTER PREDICATE [Security].[fn_tenant]([TenantId]) ON [dbo].[JobApplications],
    ADD BLOCK  PREDICATE [Security].[fn_tenant]([TenantId]) ON [dbo].[JobApplications]  AFTER INSERT

    WITH (STATE = ON, SCHEMABINDING = ON);
```

**How this works:** the leading `,` closes the previous last predicate line (which had no comma);
then all the new predicates are added; the final `JobApplications ... AFTER INSERT` has no comma
before `WITH`. Only skip a table whose migration you did NOT run.

**Done when:** the policy lists all new tables and the RLS script still parses (it runs on startup).

**⚠️ Review:** if RLS is already active in your DB, the script must be re-run (it drops/recreates the
policy at the top of `AddRLS.sql`). Confirm the startup re-applies it, then check an authenticated
manager can still read amenities/notices etc. (not empty). If something returns empty, report to Claude.
