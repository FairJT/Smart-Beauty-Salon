using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SalonOS.Catalog.Application.DTOs;

namespace SalonOS.Catalog.API.Controllers;

/// <summary>
/// Catalog service controller for managing salon services.
/// Only salon managers can manage their own services.
/// </summary>
[Route("api/catalog-services")]
[ApiController]
public class CatalogServiceController : ControllerBase
{
    // TODO: Implement catalog service service
    // For now, this is a placeholder

    [HttpGet]
    [Authorize]
    public IActionResult GetCatalogServices()
    {
        // TODO: Implement service listing for current salon
        return Ok(new List<CatalogServiceDto>());
    }

    [HttpGet("{id}")]
    [Authorize]
    public IActionResult GetCatalogService(Guid id)
    {
        // TODO: Implement service detail
        return NotFound(new { message = "Catalog service not found" });
    }

    [HttpPost]
    [Authorize]
    public IActionResult CreateCatalogService([FromBody] CreateCatalogServiceDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // TODO: Implement service creation (salon manager only)
        return CreatedAtAction(nameof(GetCatalogService), new { id = Guid.NewGuid() }, dto);
    }

    [HttpPut("{id}")]
    [Authorize]
    public IActionResult UpdateCatalogService(Guid id, [FromBody] UpdateCatalogServiceDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // TODO: Implement service update (salon manager only)
        return Ok(new { message = "Catalog service updated successfully" });
    }

    [HttpDelete("{id}")]
    [Authorize]
    public IActionResult DeleteCatalogService(Guid id)
    {
        // TODO: Implement service deletion (salon manager only)
        return Ok(new { message = "Catalog service deleted successfully" });
    }
}
