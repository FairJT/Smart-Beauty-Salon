-- ============================================================
-- SalonOS â€” Row-Level Security (RLS) Migration
-- Task 8.1  (Â§R6.6)
--
-- Run this script ONCE against the application database after
-- applying EF migrations. It creates:
--   1. Security schema
--   2. A scalar function that returns 1 when the row's TenantId
--      matches the session-context value OR the caller is PlatformOwner.
--   3. FILTER + BLOCK predicates on every tenant-owned table.
--
-- The session context is set per-request by the DbContext interceptor
-- (Task 8.2) â€” never by client input.
-- ============================================================

-- â”€â”€ 1. Security schema â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = N'Security')
    EXEC('CREATE SCHEMA [Security]');
GO

-- â”€â”€ 2. Tenant filter function â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
-- Returns 1 (allow) when:
--   a) The row's TenantId matches the session-context TenantId, OR
--   b) The session-context IsPlatformOwner flag is 1.
-- Returns 0 (deny) for all other cases, including unâ€‘authenticated connections
-- that have not set the session context.
CREATE OR ALTER FUNCTION Security.fn_tenant(@TenantId UNIQUEIDENTIFIER)
RETURNS TABLE
WITH SCHEMABINDING
AS
RETURN
    SELECT 1 AS ok
    WHERE
        -- Row belongs to the current request's tenant
        @TenantId = CAST(SESSION_CONTEXT(N'TenantId') AS UNIQUEIDENTIFIER)
        OR
        -- PlatformOwner bypass â€” set by Task 8.2 interceptor
        CAST(SESSION_CONTEXT(N'IsPlatformOwner') AS BIT) = 1;
GO

-- â”€â”€ 3. Security policy â€” drop first if reâ€‘running â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
IF EXISTS (
    SELECT 1 FROM sys.security_policies
    WHERE name = N'TenantFilter' AND schema_id = SCHEMA_ID(N'Security')
)
    DROP SECURITY POLICY [Security].[TenantFilter];
GO

-- â”€â”€ 4. Apply FILTER + BLOCK predicates to every tenantâ€‘owned table â”€â”€â”€â”€â”€â”€â”€â”€â”€
--
-- FILTER predicate : hides rows that don't belong to the current tenant on SELECT.
-- BLOCK  predicate : prevents INSERT of rows with a foreign TenantId (AFTER INSERT).
--
-- Tables covered (all inherit TenantEntity):
--   dbo.Bookings, dbo.CatalogServices, dbo.CatalogServiceOptions,
--   dbo.InventoryItems, dbo.StockMovements, dbo.SalonPackageLicenses
--
-- Add additional tenantâ€‘owned tables here as modules grow.
CREATE SECURITY POLICY [Security].[TenantFilter]
    ADD FILTER PREDICATE [Security].[fn_tenant]([TenantId]) ON [dbo].[Bookings],
    ADD BLOCK  PREDICATE [Security].[fn_tenant]([TenantId]) ON [dbo].[Bookings]  AFTER INSERT,

    ADD FILTER PREDICATE [Security].[fn_tenant]([TenantId]) ON [dbo].[CatalogServices],
    ADD BLOCK  PREDICATE [Security].[fn_tenant]([TenantId]) ON [dbo].[CatalogServices]  AFTER INSERT,

    ADD FILTER PREDICATE [Security].[fn_tenant]([TenantId]) ON [dbo].[CatalogServiceOptions],
    ADD BLOCK  PREDICATE [Security].[fn_tenant]([TenantId]) ON [dbo].[CatalogServiceOptions]  AFTER INSERT,

    ADD FILTER PREDICATE [Security].[fn_tenant]([TenantId]) ON [dbo].[InventoryItems],
    ADD BLOCK  PREDICATE [Security].[fn_tenant]([TenantId]) ON [dbo].[InventoryItems]  AFTER INSERT,

    ADD FILTER PREDICATE [Security].[fn_tenant]([TenantId]) ON [dbo].[StockMovements],
    ADD BLOCK  PREDICATE [Security].[fn_tenant]([TenantId]) ON [dbo].[StockMovements]  AFTER INSERT,

    ADD FILTER PREDICATE [Security].[fn_tenant]([TenantId]) ON [dbo].[SalonPackageLicenses],
    ADD BLOCK  PREDICATE [Security].[fn_tenant]([TenantId]) ON [dbo].[SalonPackageLicenses]  AFTER INSERT,

    ADD FILTER PREDICATE [Security].[fn_tenant]([TenantId]) ON [dbo].[ArtistSchedules],
    ADD BLOCK  PREDICATE [Security].[fn_tenant]([TenantId]) ON [dbo].[ArtistSchedules]  AFTER INSERT,

    ADD FILTER PREDICATE [Security].[fn_tenant]([TenantId]) ON [dbo].[Leaves],
    ADD BLOCK  PREDICATE [Security].[fn_tenant]([TenantId]) ON [dbo].[Leaves]  AFTER INSERT

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

    ,
    ADD FILTER PREDICATE [Security].[fn_tenant]([TenantId]) ON [dbo].[ClientNotes],
    ADD BLOCK  PREDICATE [Security].[fn_tenant]([TenantId]) ON [dbo].[ClientNotes]  AFTER INSERT,
    ADD FILTER PREDICATE [Security].[fn_tenant]([TenantId]) ON [dbo].[StaffRequests],
    ADD BLOCK  PREDICATE [Security].[fn_tenant]([TenantId]) ON [dbo].[StaffRequests]  AFTER INSERT,
    ADD FILTER PREDICATE [Security].[fn_tenant]([TenantId]) ON [dbo].[RescheduleRequests],
    ADD BLOCK  PREDICATE [Security].[fn_tenant]([TenantId]) ON [dbo].[RescheduleRequests]  AFTER INSERT,
    ADD FILTER PREDICATE [Security].[fn_tenant]([TenantId]) ON [dbo].[ProductUsages],
    ADD BLOCK  PREDICATE [Security].[fn_tenant]([TenantId]) ON [dbo].[ProductUsages]  AFTER INSERT

    ,
    ADD FILTER PREDICATE [Security].[fn_tenant]([TenantId]) ON [dbo].[ClientFeedbacks],
    ADD BLOCK  PREDICATE [Security].[fn_tenant]([TenantId]) ON [dbo].[ClientFeedbacks]  AFTER INSERT

    WITH (STATE = ON, SCHEMABINDING = ON);
GO

-- â”€â”€ 5. Verification query (run manually to confirm) â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
-- EXEC sp_set_session_context N'TenantId', '<your-test-tenant-guid>';
-- EXEC sp_set_session_context N'IsPlatformOwner', 0;
-- SELECT COUNT(*) FROM dbo.Bookings;   -- should return only that tenant's rows
-- EXEC sp_set_session_context N'IsPlatformOwner', 1;
-- SELECT COUNT(*) FROM dbo.Bookings;   -- should return ALL rows (platform owner bypass)
