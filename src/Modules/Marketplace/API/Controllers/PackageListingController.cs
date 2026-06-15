using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SalonOS.Shared.Authorization;
using SalonOS.Marketplace.Application.DTOs;

namespace SalonOS.Marketplace.API.Controllers;

/// <summary>
/// Marketplace package listing controller.
/// Browse = marketplace.browse (SalonManager).
/// Template management = marketplace.template.manage (PlatformOwner only).
/// </summary>
[Route("api/package-listings")]
[ApiController]
public class PackageListingController : ControllerBase
{
    [HttpGet]
    [HasPermission(Permissions.MarketplaceBrowse)]
    public IActionResult GetPackageListings() => Ok(new List<PackageListingDto>());

    [HttpGet("{id}")]
    [HasPermission(Permissions.MarketplaceBrowse)]
    public IActionResult GetPackageListing(Guid id) =>
        NotFound(new { message = "Package listing not found" });

    [HttpPost]
    [HasPermission(Permissions.MarketplaceTemplateManage)]
    public IActionResult CreatePackageListing([FromBody] CreatePackageListingDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        return CreatedAtAction(nameof(GetPackageListing), new { id = Guid.NewGuid() }, dto);
    }
}
