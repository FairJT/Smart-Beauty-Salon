using SalonOS.Shared;

namespace SalonOS.Infrastructure.EventHandlers;

/// <summary>
/// Handler for PackagePurchased domain event.
/// Provisions catalog services when a package is purchased.
/// </summary>
public class PackagePurchasedHandler
{
    // TODO: Inject required services
    // private readonly ICatalogService _catalogService;

    public async Task HandleAsync(PackagePurchased domainEvent)
    {
        // TODO: Implement handler logic
        // 1. Provision catalog services based on purchased package
        // 2. Activate services for the salon
        // 3. Send confirmation notification
        
        await Task.CompletedTask;
    }
}

/// <summary>
/// Package purchased domain event.
/// </summary>
public class PackagePurchased : DomainEvent
{
    public Guid TenantId { get; }
    public Guid PackageListingId { get; }
    public Money PaidAmount { get; }

    public PackagePurchased(Guid tenantId, Guid packageListingId, Money paidAmount)
    {
        TenantId = tenantId;
        PackageListingId = packageListingId;
        PaidAmount = paidAmount;
    }

    public override string EventType => nameof(PackagePurchased);
}
