using Microsoft.AspNetCore.Mvc;
using SalonOS.Shared.Authorization;
using SalonOS.Inventory.Application.DTOs;
using SalonOS.Shared.Authorization;

namespace SalonOS.Inventory.API.Controllers;

/// <summary>
/// Inventory controller.
/// inventory.view — SalonManager, Receptionist.
/// inventory.adjust / inventory.manage — SalonManager only.
/// Authorize on permission strings — never on role names (R2).
/// </summary>
[Route("api/inventory-items")]
[ApiController]
public class InventoryItemController : ControllerBase
{
    [HttpGet]
    [HasPermission(Permissions.InventoryView)]
    public IActionResult GetInventoryItems() => Ok(new List<InventoryItemDto>());

    [HttpGet("{id}")]
    [HasPermission(Permissions.InventoryView)]
    public IActionResult GetInventoryItem(Guid id) =>
        NotFound(new { message = "Inventory item not found" });

    [HttpPost]
    [HasPermission(Permissions.InventoryManage)]
    public IActionResult CreateInventoryItem([FromBody] CreateInventoryItemDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        return CreatedAtAction(nameof(GetInventoryItem), new { id = Guid.NewGuid() }, dto);
    }

    [HttpPut("{id}")]
    [HasPermission(Permissions.InventoryManage)]
    public IActionResult UpdateInventoryItem(Guid id, [FromBody] UpdateInventoryItemDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        return Ok(new { message = "Inventory item updated successfully" });
    }

    [HttpPost("{id}/adjust")]
    [HasPermission(Permissions.InventoryAdjust)]
    public IActionResult AdjustStock(Guid id) =>
        Ok(new { message = "Stock adjusted" });
}
