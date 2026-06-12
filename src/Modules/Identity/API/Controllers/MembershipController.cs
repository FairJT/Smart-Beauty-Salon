using Microsoft.AspNetCore.Mvc;
using SalonOS.Api.Authorization;
using SalonOS.Identity.Application.DTOs;
using SalonOS.Shared.Authorization;

namespace SalonOS.Identity.API.Controllers;

/// <summary>
/// Membership (Staff / Artist) controller.
/// Task 6.3: staff.* permissions per §R4.
/// performance = staff.performance.view.
/// Authorize on permission strings — never on role names (R2).
/// </summary>
[Route("api/memberships")]
[ApiController]
public class MembershipController : ControllerBase
{
    // ── GET /api/memberships — staff.view ─────────────────────────────────────
    [HttpGet]
    [HasPermission(Permissions.StaffView)]
    public IActionResult GetMemberships()
    {
        return Ok(new List<MembershipDto>());
    }

    // ── GET /api/memberships/{id} — staff.view ────────────────────────────────
    [HttpGet("{id}")]
    [HasPermission(Permissions.StaffView)]
    public IActionResult GetMembership(Guid id)
    {
        return NotFound(new { message = "Membership not found" });
    }

    // ── POST /api/memberships — staff.create ──────────────────────────────────
    [HttpPost]
    [HasPermission(Permissions.StaffCreate)]
    public IActionResult CreateMembership([FromBody] CreateMembershipDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        return CreatedAtAction(nameof(GetMembership), new { id = Guid.NewGuid() }, dto);
    }

    // ── PUT /api/memberships/{id} — staff.edit ────────────────────────────────
    [HttpPut("{id}")]
    [HasPermission(Permissions.StaffEdit)]
    public IActionResult UpdateMembership(Guid id, [FromBody] CreateMembershipDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        return Ok(new { message = "Membership updated successfully" });
    }

    // ── DELETE /api/memberships/{id} — staff.delete ───────────────────────────
    [HttpDelete("{id}")]
    [HasPermission(Permissions.StaffDelete)]
    public IActionResult DeleteMembership(Guid id)
    {
        return Ok(new { message = "Membership deleted successfully" });
    }

    // ── PUT /api/memberships/{id}/contract — staff.contract.manage ───────────
    [HttpPut("{id}/contract")]
    [HasPermission(Permissions.StaffContractManage)]
    public IActionResult ManageContract(Guid id)
    {
        return Ok(new { message = "Contract updated successfully" });
    }

    // ── GET /api/memberships/{id}/performance — staff.performance.view ────────
    [HttpGet("{id}/performance")]
    [HasPermission(Permissions.StaffPerformanceView)]
    public IActionResult GetPerformance(Guid id)
    {
        return Ok(new { message = "Performance data" });
    }
}
