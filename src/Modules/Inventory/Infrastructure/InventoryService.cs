using Microsoft.EntityFrameworkCore;
using SalonOS.Inventory.Domain;
using SalonOS.Shared;

namespace SalonOS.Inventory.Infrastructure;

/// <summary>
/// Interface for inventory service.
/// </summary>
public interface IInventoryService
{
    Task<InventoryItem?> GetByIdAsync(Guid id, Guid tenantId);
    Task<List<InventoryItem>> GetByTenantIdAsync(Guid tenantId);
    Task<InventoryItem> CreateAsync(InventoryItem item);
    Task UpdateAsync(InventoryItem item);
    Task<StockMovement> AddMovementAsync(StockMovement movement);
    Task<decimal> GetOnHandQtyAsync(Guid itemId, Guid tenantId);
}

/// <summary>
/// Inventory service implementation.
/// Handles inventory operations and business logic.
/// </summary>
public class InventoryService : IInventoryService
{
    private readonly InventoryDbContext _context;

    public InventoryService(InventoryDbContext context)
    {
        _context = context;
    }

    public async Task<InventoryItem?> GetByIdAsync(Guid id, Guid tenantId)
    {
        return await _context.InventoryItems
            .FirstOrDefaultAsync(i => i.Id == id && i.TenantId == tenantId);
    }

    public async Task<List<InventoryItem>> GetByTenantIdAsync(Guid tenantId)
    {
        return await _context.InventoryItems
            .Where(i => i.TenantId == tenantId && i.IsActive)
            .ToListAsync();
    }

    public async Task<InventoryItem> CreateAsync(InventoryItem item)
    {
        _context.InventoryItems.Add(item);
        await _context.SaveChangesAsync();
        return item;
    }

    public async Task UpdateAsync(InventoryItem item)
    {
        _context.InventoryItems.Update(item);
        await _context.SaveChangesAsync();
    }

    public async Task<StockMovement> AddMovementAsync(StockMovement movement)
    {
        // Add the movement
        _context.StockMovements.Add(movement);

        // Update on-hand quantity
        var item = await _context.InventoryItems
            .FirstOrDefaultAsync(i => i.Id == movement.InventoryItemId);

        if (item != null)
        {
            item.OnHandQty += movement.Quantity;
            _context.InventoryItems.Update(item);

            // Check for low inventory
            if (item.OnHandQty <= item.ReorderThreshold)
            {
                // TODO: Raise InventoryLow domain event
                // This will be handled by the outbox pattern
            }
        }

        await _context.SaveChangesAsync();
        return movement;
    }

    public async Task<decimal> GetOnHandQtyAsync(Guid itemId, Guid tenantId)
    {
        var item = await _context.InventoryItems
            .FirstOrDefaultAsync(i => i.Id == itemId && i.TenantId == tenantId);

        return item?.OnHandQty ?? 0;
    }
}
