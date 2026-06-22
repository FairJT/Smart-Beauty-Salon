# CZ — Put `ClientFeedbacks` under RLS 🟡 (do LAST)

After C2 exists. It's already tenant-scoped by `AppDbContext` (Layer 2); this adds the RLS backstop.

**File:** `src/SalonOS.Infrastructure/Migrations/AddRLS.sql`

**Find (exact):**
```sql
    WITH (STATE = ON, SCHEMABINDING = ON);
```

**Replace with:**
```sql
    ,
    ADD FILTER PREDICATE [Security].[fn_tenant]([TenantId]) ON [dbo].[ClientFeedbacks],
    ADD BLOCK  PREDICATE [Security].[fn_tenant]([TenantId]) ON [dbo].[ClientFeedbacks]  AFTER INSERT

    WITH (STATE = ON, SCHEMABINDING = ON);
```
(The leading `,` closes the previous last predicate; the new `ClientFeedbacks ... AFTER INSERT` has no
comma before `WITH`.)

**Done when:** the policy lists `ClientFeedbacks` and the script still parses.
