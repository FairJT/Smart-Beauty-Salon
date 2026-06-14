using SalonOS.Shared;

namespace SalonOS.Catalog.Domain;

public class SalonMedia : TenantEntity
{
    public Guid SalonId { get; set; }
    public string Url { get; set; } = string.Empty;
    public string MediaType { get; set; } = "image";
    public int SortOrder { get; set; }
    public string? AltText { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
