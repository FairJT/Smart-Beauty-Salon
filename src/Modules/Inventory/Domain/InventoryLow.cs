using SalonOS.Shared;

namespace SalonOS.Inventory.Domain;

/// <summary>
/// Inventory low domain event.
/// Raised when an inventory item's on-hand quantity falls below the reorder threshold.
/// </summary>
public class InventoryLow : DomainEvent
{
    public Guid InventoryItemId { get; }
    public string ItemName { get; }
    public decimal CurrentQty { get; }
    public decimal ReorderThreshold { get; }
    public Guid TenantId { get; }

    public InventoryLow(Guid inventoryItemId, string itemName, decimal currentQty, decimal reorderThreshold, Guid tenantId)
    {
        InventoryItemId = inventoryItemId;
        ItemName = itemName;
        CurrentQty = currentQty;
        ReorderThreshold = reorderThreshold;
        TenantId = tenantId;
    }

    public override string EventType => nameof(InventoryLow);
}
