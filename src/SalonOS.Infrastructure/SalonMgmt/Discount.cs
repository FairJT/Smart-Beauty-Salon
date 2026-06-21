using SalonOS.Shared;

namespace SalonOS.Infrastructure;

public class Discount : TenantEntity
{
    public string? Code { get; set; }            // null = general (auto), set = coupon code
    public int? Percent { get; set; }            // either Percent ...
    public long? AmountMinor { get; set; }       // ... or a fixed amount (Rial minor units)
    public DateTime StartsAt { get; set; }
    public DateTime EndsAt { get; set; }
    public string? TargetClientId { get; set; }  // null = anyone; set = one client
    public int? MaxUses { get; set; }
    public int UsedCount { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}