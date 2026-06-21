using SalonOS.Shared;

namespace SalonOS.Infrastructure;

public enum StaffContractKind { Percentage = 1, Rental = 2, FixedSalary = 3 }

public class StaffServiceContract : TenantEntity
{
    public Guid ArtistId { get; set; }
    public Guid CatalogServiceId { get; set; }
    public StaffContractKind Kind { get; set; } = StaffContractKind.Rental;
    public Money Amount { get; set; } = Money.Zero("IRR");
    public int? DiscountPercent { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? ContractFileUrl { get; set; }
    public string? GuaranteeNote { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}