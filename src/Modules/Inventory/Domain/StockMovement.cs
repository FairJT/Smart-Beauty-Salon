using SalonOS.Shared;

namespace SalonOS.Inventory.Domain;

/// <summary>
/// Stock movement entity - TENANT entity.
/// Append-only ledger tracking all inventory changes.
/// </summary>
public class StockMovement : TenantEntity
{
    public Guid InventoryItemId { get; set; }
    public StockMovementType Type { get; set; }
    public decimal Quantity { get; set; }
    public string? Reference { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public InventoryItem InventoryItem { get; set; } = null!;
}

/// <summary>
/// Stock movement type.
/// </summary>
public enum StockMovementType
{
    Purchase = 1,
    Sale = 2,
    Adjustment = 3,
    Return = 4,
   损耗 = 5 // Damage/Loss
}
