using SalonOS.Shared;

namespace SalonOS.Infrastructure;

public class ProductUsage : TenantEntity
{
    public Guid BookingId { get; set; }
    public Guid ArtistId { get; set; }
    public Guid InventoryItemId { get; set; }
    public decimal Quantity { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}