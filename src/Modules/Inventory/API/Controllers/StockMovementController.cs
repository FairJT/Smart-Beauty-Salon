using Microsoft.AspNetCore.Mvc;
using SalonOS.Shared.Authorization;
using SalonOS.Inventory.Application.DTOs;

namespace SalonOS.Inventory.API.Controllers;

/// <summary>
/// Stock movement controller.
/// inventory.view to list; inventory.adjust to create movements (R2).
/// </summary>
[Route("api/stock-movements")]
[ApiController]
public class StockMovementController : ControllerBase
{
    [HttpGet]
    [HasPermission(Permissions.InventoryView)]
    public IActionResult GetStockMovements([FromQuery] Guid? inventoryItemId = null) =>
        Ok(new List<StockMovementDto>());

    [HttpPost]
    [HasPermission(Permissions.InventoryAdjust)]
    public IActionResult CreateStockMovement([FromBody] CreateStockMovementDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        return CreatedAtAction(nameof(GetStockMovements), new { }, dto);
    }
}
