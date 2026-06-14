using SalonOS.Shared;

namespace SalonOS.Catalog.Domain;

public class ServiceOption : TenantEntity
{
    public Guid CatalogServiceId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Money PriceDelta { get; set; } = Money.Zero("IRR");
    public int DurationDeltaMinutes { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public CatalogService CatalogService { get; set; } = null!;
}
