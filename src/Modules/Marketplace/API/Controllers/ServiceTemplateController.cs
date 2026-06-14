using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SalonOS.Marketplace.Application.DTOs;

namespace SalonOS.Marketplace.API.Controllers;

/// <summary>
/// Service template controller for managing service templates.
/// Only platform owners can manage templates.
/// </summary>
[Route("api/service-templates")]
[ApiController]
public class ServiceTemplateController : ControllerBase
{
    // TODO: Implement service template service
    // For now, this is a placeholder

    [HttpGet]
    public IActionResult GetServiceTemplates()
    {
        // TODO: Implement template listing
        return Ok(new List<ServiceTemplateDto>());
    }

    [HttpGet("{id}")]
    public IActionResult GetServiceTemplate(Guid id)
    {
        // TODO: Implement template detail
        return NotFound(new { message = "Service template not found" });
    }

    [HttpPost]
    [Authorize]
    public IActionResult CreateServiceTemplate([FromBody] CreateServiceTemplateDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // TODO: Implement template creation (platform owner only)
        return CreatedAtAction(nameof(GetServiceTemplate), new { id = Guid.NewGuid() }, dto);
    }

    [HttpPut("{id}")]
    [Authorize]
    public IActionResult UpdateServiceTemplate(Guid id, [FromBody] UpdateServiceTemplateDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // TODO: Implement template update (platform owner only)
        return Ok(new { message = "Service template updated successfully" });
    }
}
