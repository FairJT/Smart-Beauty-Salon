using SalonOS.Shared;

namespace SalonOS.Marketplace.Domain;

/// <summary>
/// Salon package license entity - TENANT entity.
/// Tracks which packages a salon has purchased.
/// </summary>
public class SalonPackageLicense : TenantEntity
{
    public Guid PackageListingId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; } = true;
    public Money PaidAmount { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public PackageListing PackageListing { get; set; } = null!;
}
