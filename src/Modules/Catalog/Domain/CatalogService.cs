using SalonOS.Shared;

namespace SalonOS.Catalog.Domain;

public class CatalogService : TenantEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid ServiceTypeId { get; set; }
    public Money BasePrice { get; set; } = Money.Zero("IRR");
    public int BaseDurationMinutes { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ServiceType ServiceType { get; set; } = null!;
    public ICollection<ServiceOption> Options { get; set; } = new List<ServiceOption>();
    public ICollection<Material> Materials { get; set; } = new List<Material>();
}
