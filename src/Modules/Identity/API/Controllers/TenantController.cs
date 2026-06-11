using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SalonOS.Identity.Application.DTOs;

namespace SalonOS.Identity.API.Controllers;

/// <summary>
/// Tenant controller for managing tenants (salons/businesses).
/// </summary>
[Route("api/tenants")]
[ApiController]
public class TenantController : ControllerBase
{
    // TODO: Implement tenant service
    // For now, this is a placeholder

    [HttpGet]
    [Authorize]
    public IActionResult GetTenants()
    {
        // TODO: Implement tenant listing
        return Ok(new List<TenantDto>());
    }

    [HttpGet("{id}")]
    [Authorize]
    public IActionResult GetTenant(Guid id)
    {
        // TODO: Implement tenant detail
        return NotFound(new { message = "Tenant not found" });
    }

    [HttpPost]
    [Authorize]
    public IActionResult CreateTenant([FromBody] CreateTenantDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // TODO: Implement tenant creation
        return CreatedAtAction(nameof(GetTenant), new { id = Guid.NewGuid() }, dto);
    }

    [HttpPut("{id}")]
    [Authorize]
    public IActionResult UpdateTenant(Guid id, [FromBody] UpdateTenantDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // TODO: Implement tenant update
        return Ok(new { message = "Tenant updated successfully" });
    }
}
