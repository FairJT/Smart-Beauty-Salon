using SalonOS.Shared;

namespace SalonOS.Marketplace.Domain;

/// <summary>
/// Package listing entity - GLOBAL entity (no TenantId).
/// Defines packages that salons can purchase.
/// </summary>
public class PackageListing
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Category { get; set; } = string.Empty;
    public Money Price { get; set; }
    public int DurationMonths { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
