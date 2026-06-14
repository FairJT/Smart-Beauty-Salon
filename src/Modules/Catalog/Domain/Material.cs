using SalonOS.Shared;

namespace SalonOS.Catalog.Domain;

public class Material : TenantEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Money Price { get; set; } = Money.Zero("IRR");
    public string? Unit { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<CatalogService> SalonServices { get; set; } = new List<CatalogService>();
}
