-- ============================================================
-- SalonOS — Row-Level Security (RLS) Migration
-- Task 8.1  (§R6.6)
--
-- Run this script ONCE against the application database after
-- applying EF migrations. It creates:
--   1. Security schema
--   2. A scalar function that returns 1 when the row's TenantId
--      matches the session-context value OR the caller is PlatformOwner.
--   3. FILTER + BLOCK predicates on every tenant-owned table.
--
-- The session context is set per-request by the DbContext interceptor
-- (Task 8.2) — never by client input.
-- ============================================================

-- ── 1. Security schema ─────────────────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = N'Security')
    EXEC('CREATE SCHEMA [Security]');
GO

-- ── 2. Tenant filter function ──────────────────────────────────────────────
-- Returns 1 (allow) when:
--   a) The row's TenantId matches the session-context TenantId, OR
--   b) The session-context IsPlatformOwner flag is 1.
-- Returns 0 (deny) for all other cases, including unauthenticated connections
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
        -- PlatformOwner bypass — set by Task 8.2 interceptor
        CAST(SESSION_CONTEXT(N'IsPlatformOwner') AS BIT) = 1;
GO

-- ── 3. Security policy — drop first if re-running ─────────────────────────
IF EXISTS (
    SELECT 1 FROM sys.security_policies
    WHERE name = N'TenantFilter' AND schema_id = SCHEMA_ID(N'Security')
)
    DROP SECURITY POLICY [Security].[TenantFilter];
GO

-- ── 4. Apply FILTER + BLOCK predicates to every tenant-owned table ─────────
--
-- FILTER predicate : hides rows that don't belong to the current tenant on SELECT.
-- BLOCK  predicate : prevents INSERT of rows with a foreign TenantId (AFTER INSERT).
--
-- Tables covered (all inherit TenantEntity):
--   dbo.Bookings, dbo.CatalogServices, dbo.CatalogServiceOptions,
--   dbo.InventoryItems, dbo.StockMovements, dbo.SalonPackageLicenses
--
-- Add additional tenant-owned tables here as modules grow.
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
    ADD BLOCK  PREDICATE [Security].[fn_tenant]([TenantId]) ON [dbo].[SalonPackageLicenses]  AFTER INSERT

    WITH (STATE = ON, SCHEMABINDING = ON);
GO

-- ── 5. Verification query (run manually to confirm) ───────────────────────
-- EXEC sp_set_session_context N'TenantId', '<your-test-tenant-guid>';
-- EXEC sp_set_session_context N'IsPlatformOwner', 0;
-- SELECT COUNT(*) FROM dbo.Bookings;   -- should return only that tenant's rows
-- EXEC sp_set_session_context N'IsPlatformOwner', 1;
-- SELECT COUNT(*) FROM dbo.Bookings;   -- should return ALL rows (platform owner bypass)
