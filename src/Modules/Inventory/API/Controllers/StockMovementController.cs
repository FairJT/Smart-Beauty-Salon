using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SalonOS.Inventory.Application.DTOs;

namespace SalonOS.Inventory.API.Controllers;

/// <summary>
/// Stock movement controller for viewing inventory movements.
/// </summary>
[Route("api/stock-movements")]
[ApiController]
public class StockMovementController : ControllerBase
{
    // TODO: Implement stock movement service
    // For now, this is a placeholder

    [HttpGet]
    [Authorize]
    public IActionResult GetStockMovements([FromQuery] Guid? inventoryItemId = null)
    {
        // TODO: Implement movement listing
        return Ok(new List<StockMovementDto>());
    }

    [HttpPost]
    [Authorize]
    public IActionResult CreateStockMovement([FromBody] CreateStockMovementDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // TODO: Implement movement creation
        // This should update OnHandQty and check for low inventory
        return CreatedAtAction(nameof(GetStockMovements), new { }, dto);
    }
}
