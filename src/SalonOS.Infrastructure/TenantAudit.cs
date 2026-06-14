namespace SalonOS.Infrastructure;

/// <summary>
/// Tenant audit helper for tracking tenant scoping.
/// Documents which entities need tenant scoping and which queries need updating.
/// </summary>
public static class TenantAudit
{
    /// <summary>
    /// Entities that require tenant scoping (have TenantId).
    /// </summary>
    public static readonly string[] TenantScopedEntities = new[]
    {
        "CatalogService",
        "CatalogServiceOption",
        "InventoryItem",
        "StockMovement",
        "Booking",
        "SalonPackageLicense"
    };

    /// <summary>
    /// Services that need query updates for tenant scoping.
    /// </summary>
    public static readonly string[] ServicesNeedingUpdates = new[]
    {
        "CatalogService (Catalog module)",
        "InventoryService (Inventory module)",
        "BookingService (Booking module)",
        "MarketplaceService (Marketplace module)"
    };

    /// <summary>
    /// Queries that need to be updated to filter by TenantId.
    /// </summary>
    public static readonly string[] QueriesNeedingUpdates = new[]
    {
        "GetAll -> Filter by TenantId",
        "GetById -> Include TenantId check",
        "Create -> Set TenantId from context",
        "Update -> Verify TenantId matches",
        "Delete -> Verify TenantId matches"
    };
}
