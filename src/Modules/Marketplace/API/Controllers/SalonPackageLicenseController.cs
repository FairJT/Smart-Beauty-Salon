using Microsoft.AspNetCore.Mvc;
using SalonOS.Shared.Authorization;
using SalonOS.Marketplace.Application.DTOs;
using SalonOS.Shared.Authorization;

namespace SalonOS.Marketplace.API.Controllers;

/// <summary>
/// Salon package license (purchase) controller.
/// marketplace.browse to list; marketplace.license.purchase to buy (SalonManager).
/// </summary>
[Route("api/salon-package-licenses")]
[ApiController]
public class SalonPackageLicenseController : ControllerBase
{
    [HttpGet]
    [HasPermission(Permissions.MarketplaceBrowse)]
    public IActionResult GetSalonPackageLicenses() => Ok(new List<SalonPackageLicenseDto>());

    [HttpPost]
    [HasPermission(Permissions.MarketplaceLicensePurchase)]
    public IActionResult PurchasePackage([FromBody] CreateSalonPackageLicenseDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        return CreatedAtAction(nameof(GetSalonPackageLicenses), new { }, dto);
    }
}

/// <summary>DTO for creating a salon package license.</summary>
public class CreateSalonPackageLicenseDto
{
    public Guid PackageListingId { get; set; }
}
