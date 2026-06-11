using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SalonOS.Marketplace.Application.DTOs;

namespace SalonOS.Marketplace.API.Controllers;

/// <summary>
/// Package listing controller for managing packages.
/// Public read access, platform owners can manage.
/// </summary>
[Route("api/package-listings")]
[ApiController]
public class PackageListingController : ControllerBase
{
    // TODO: Implement package listing service
    // For now, this is a placeholder

    [HttpGet]
    public IActionResult GetPackageListings()
    {
        // TODO: Implement package listing
        return Ok(new List<PackageListingDto>());
    }

    [HttpGet("{id}")]
    public IActionResult GetPackageListing(Guid id)
    {
        // TODO: Implement package detail
        return NotFound(new { message = "Package listing not found" });
    }

    [HttpPost]
    [Authorize]
    public IActionResult CreatePackageListing([FromBody] CreatePackageListingDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // TODO: Implement package creation (platform owner only)
        return CreatedAtAction(nameof(GetPackageListing), new { id = Guid.NewGuid() }, dto);
    }
}
