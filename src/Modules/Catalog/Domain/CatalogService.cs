using SalonOS.Shared;

namespace SalonOS.Catalog.Domain;

/// <summary>
/// Catalog service entity - TENANT entity.
/// Represents a service offered by a specific salon.
/// </summary>
public class CatalogService : TenantEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Category { get; set; } = string.Empty;
    public int BaseDurationMinutes { get; set; }
    public Money BasePrice { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public ICollection<CatalogServiceOption> Options { get; set; } = new List<CatalogServiceOption>();
}
