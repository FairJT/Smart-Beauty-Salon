using SalonOS.Shared;

namespace SalonOS.Inventory.Domain;

/// <summary>
/// Inventory item entity - TENANT entity.
/// Tracks inventory items for a salon.
/// </summary>
public class InventoryItem : TenantEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Category { get; set; } = string.Empty;
    public string? Sku { get; set; }
    public decimal OnHandQty { get; set; }
    public decimal ReorderThreshold { get; set; }
    public Money UnitCost { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public ICollection<StockMovement> StockMovements { get; set; } = new List<StockMovement>();
}
