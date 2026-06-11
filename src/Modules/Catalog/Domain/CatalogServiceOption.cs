using SalonOS.Shared;

namespace SalonOS.Catalog.Domain;

/// <summary>
/// Catalog service option entity - TENANT entity.
/// Represents an option for a specific service.
/// </summary>
public class CatalogServiceOption : TenantEntity
{
    public Guid CatalogServiceId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public CatalogService CatalogService { get; set; } = null!;
}
