using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SalonOS.Identity.Application.DTOs;

namespace SalonOS.Identity.API.Controllers;

/// <summary>
/// Membership controller for managing user-tenant relationships.
/// </summary>
[Route("api/memberships")]
[ApiController]
public class MembershipController : ControllerBase
{
    // TODO: Implement membership service
    // For now, this is a placeholder

    [HttpGet]
    [Authorize]
    public IActionResult GetMemberships()
    {
        // TODO: Implement membership listing
        return Ok(new List<MembershipDto>());
    }

    [HttpPost]
    [Authorize]
    public IActionResult CreateMembership([FromBody] CreateMembershipDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // TODO: Implement membership creation
        return CreatedAtAction(nameof(GetMemberships), new { }, dto);
    }

    [HttpDelete("{id}")]
    [Authorize]
    public IActionResult DeleteMembership(Guid id)
    {
        // TODO: Implement membership deletion
        return Ok(new { message = "Membership deleted successfully" });
    }
}
