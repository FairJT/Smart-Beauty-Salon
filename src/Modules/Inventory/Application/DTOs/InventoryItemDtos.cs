using System.ComponentModel.DataAnnotations;

namespace SalonOS.Inventory.Application.DTOs;

/// <summary>
/// DTO for creating an inventory item.
/// </summary>
public class CreateInventoryItemDto
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [Required]
    [MaxLength(100)]
    public string Category { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? Sku { get; set; }

    [Required]
    [Range(0, double.MaxValue)]
    public decimal OnHandQty { get; set; }

    [Required]
    [Range(0, double.MaxValue)]
    public decimal ReorderThreshold { get; set; }

    [Required]
    public long UnitCostAmount { get; set; }

    [Required]
    [MaxLength(3)]
    public string UnitCostCurrency { get; set; } = "IRR";
}

/// <summary>
/// DTO for updating an inventory item.
/// </summary>
public class UpdateInventoryItemDto
{
    [MaxLength(100)]
    public string? Name { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    [MaxLength(100)]
    public string? Category { get; set; }

    [MaxLength(50)]
    public string? Sku { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? ReorderThreshold { get; set; }

    public long? UnitCostAmount { get; set; }

    [MaxLength(3)]
    public string? UnitCostCurrency { get; set; }
}

/// <summary>
/// DTO for inventory item response.
/// </summary>
public class InventoryItemDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Category { get; set; } = string.Empty;
    public string? Sku { get; set; }
    public decimal OnHandQty { get; set; }
    public decimal ReorderThreshold { get; set; }
    public long UnitCostAmount { get; set; }
    public string UnitCostCurrency { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// DTO for stock movement.
/// </summary>
public class StockMovementDto
{
    public Guid Id { get; set; }
    public Guid InventoryItemId { get; set; }
    public string Type { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public string? Reference { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// DTO for creating a stock movement.
/// </summary>
public class CreateStockMovementDto
{
    [Required]
    public Guid InventoryItemId { get; set; }

    [Required]
    public string Type { get; set; } = string.Empty;

    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal Quantity { get; set; }

    [MaxLength(100)]
    public string? Reference { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }
}
