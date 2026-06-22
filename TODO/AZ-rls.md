# AZ — Put the new artist tables under RLS 🟡 (do LAST)

After A2–A5 entities exist. They're already tenant-scoped by `AppDbContext` (Layer 2); this adds the
RLS backstop (Layer 3). Only include a table whose migration you actually ran.

**File:** `src/SalonOS.Infrastructure/Migrations/AddRLS.sql`

**Find (exact):**
```sql
    WITH (STATE = ON, SCHEMABINDING = ON);
```

**Replace with:**
```sql
    ,
    ADD FILTER PREDICATE [Security].[fn_tenant]([TenantId]) ON [dbo].[ClientNotes],
    ADD BLOCK  PREDICATE [Security].[fn_tenant]([TenantId]) ON [dbo].[ClientNotes]  AFTER INSERT,
    ADD FILTER PREDICATE [Security].[fn_tenant]([TenantId]) ON [dbo].[StaffRequests],
    ADD BLOCK  PREDICATE [Security].[fn_tenant]([TenantId]) ON [dbo].[StaffRequests]  AFTER INSERT,
    ADD FILTER PREDICATE [Security].[fn_tenant]([TenantId]) ON [dbo].[RescheduleRequests],
    ADD BLOCK  PREDICATE [Security].[fn_tenant]([TenantId]) ON [dbo].[RescheduleRequests]  AFTER INSERT,
    ADD FILTER PREDICATE [Security].[fn_tenant]([TenantId]) ON [dbo].[ProductUsages],
    ADD BLOCK  PREDICATE [Security].[fn_tenant]([TenantId]) ON [dbo].[ProductUsages]  AFTER INSERT

    WITH (STATE = ON, SCHEMABINDING = ON);
```
(The leading `,` closes the previous last predicate; the final `ProductUsages ... AFTER INSERT` has no
comma before `WITH`.)

**Done when:** the policy lists the 4 new tables and the script still parses.

**⚠️ Review:** `Booking.CheckedInAt` lives in the `Bookings` table, which is ALREADY under RLS — no change
needed for it. After deploy, confirm an authenticated artist can read their notes/requests (not empty).
