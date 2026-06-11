using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SalonOS.Marketplace.Application.DTOs;

namespace SalonOS.Marketplace.API.Controllers;

/// <summary>
/// Salon package license controller for managing salon package subscriptions.
/// Only salon managers can manage their own licenses.
/// </summary>
[Route("api/salon-package-licenses")]
[ApiController]
public class SalonPackageLicenseController : ControllerBase
{
    // TODO: Implement salon package license service
    // For now, this is a placeholder

    [HttpGet]
    [Authorize]
    public IActionResult GetSalonPackageLicenses()
    {
        // TODO: Implement license listing for current salon
        return Ok(new List<SalonPackageLicenseDto>());
    }

    [HttpPost]
    [Authorize]
    public IActionResult PurchasePackage([FromBody] CreateSalonPackageLicenseDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // TODO: Implement package purchase (salon manager only)
        // This should also handle payment via IPaymentProvider
        return CreatedAtAction(nameof(GetSalonPackageLicenses), new { }, dto);
    }
}

/// <summary>
/// DTO for creating a salon package license.
/// </summary>
public class CreateSalonPackageLicenseDto
{
    public Guid PackageListingId { get; set; }
}
