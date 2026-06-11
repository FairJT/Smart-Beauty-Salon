using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SalonOS.Inventory.Application.DTOs;

namespace SalonOS.Inventory.API.Controllers;

/// <summary>
/// Inventory item controller for managing salon inventory.
/// Only salon managers can manage their own inventory.
/// </summary>
[Route("api/inventory-items")]
[ApiController]
public class InventoryItemController : ControllerBase
{
    // TODO: Implement inventory item service
    // For now, this is a placeholder

    [HttpGet]
    [Authorize]
    public IActionResult GetInventoryItems()
    {
        // TODO: Implement inventory listing for current salon
        return Ok(new List<InventoryItemDto>());
    }

    [HttpGet("{id}")]
    [Authorize]
    public IActionResult GetInventoryItem(Guid id)
    {
        // TODO: Implement inventory detail
        return NotFound(new { message = "Inventory item not found" });
    }

    [HttpPost]
    [Authorize]
    public IActionResult CreateInventoryItem([FromBody] CreateInventoryItemDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // TODO: Implement inventory creation (salon manager only)
        return CreatedAtAction(nameof(GetInventoryItem), new { id = Guid.NewGuid() }, dto);
    }

    [HttpPut("{id}")]
    [Authorize]
    public IActionResult UpdateInventoryItem(Guid id, [FromBody] UpdateInventoryItemDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // TODO: Implement inventory update (salon manager only)
        return Ok(new { message = "Inventory item updated successfully" });
    }
}
