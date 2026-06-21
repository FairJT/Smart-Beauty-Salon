using SalonOS.Shared;

namespace SalonOS.Infrastructure;

public class SalonAmenity : TenantEntity
{
    public string Name { get; set; } = string.Empty;   // پارکینگ، فضای اسموک، کافی‌بار ...
    public string? Icon { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}