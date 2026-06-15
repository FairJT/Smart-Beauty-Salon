using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SalonOS.Shared.Authorization;
using SalonOS.Identity.Application.DTOs;

namespace SalonOS.Identity.API.Controllers;

/// <summary>
/// Tenant (Salon) controller.
/// Task 6.2: salon.view / salon.edit / salon.settings.manage per §R4.
/// Tenant listing (GET all) requires tenant.manage — PlatformOwner only;
/// that cross-tenant path is enforced in PlatformAdminService (Task 7.1).
/// </summary>
[Route("api/tenants")]
[ApiController]
public class TenantController : ControllerBase
{
    // ── GET /api/tenants — PlatformOwner only (cross-tenant list) ─────────────
    [HttpGet]
    [HasPermission(Permissions.TenantManage)]
    public IActionResult GetTenants()
    {
        // Full implementation in Task 7.1 (PlatformAdminService)
        return Ok(new List<TenantDto>());
    }

    // ── GET /api/tenants/{id} — any authenticated user with salon.view ────────
    [HttpGet("{id}")]
    [HasPermission(Permissions.SalonView)]
    public IActionResult GetTenant(Guid id)
    {
        return NotFound(new { message = "Tenant not found" });
    }

    // ── POST /api/tenants — PlatformOwner: tenant.manage ─────────────────────
    [HttpPost]
    [HasPermission(Permissions.TenantManage)]
    public IActionResult CreateTenant([FromBody] CreateTenantDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        return CreatedAtAction(nameof(GetTenant), new { id = Guid.NewGuid() }, dto);
    }

    // ── PUT /api/tenants/{id} — SalonManager: salon.edit ─────────────────────
    [HttpPut("{id}")]
    [HasPermission(Permissions.SalonEdit)]
    public IActionResult UpdateTenant(Guid id, [FromBody] UpdateTenantDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        return Ok(new { message = "Tenant updated successfully" });
    }

    // ── PUT /api/tenants/{id}/settings — SalonManager: salon.settings.manage ──
    [HttpPut("{id}/settings")]
    [HasPermission(Permissions.SalonSettingsManage)]
    public IActionResult UpdateSettings(Guid id, [FromBody] UpdateTenantDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        return Ok(new { message = "Settings updated successfully" });
    }
}
