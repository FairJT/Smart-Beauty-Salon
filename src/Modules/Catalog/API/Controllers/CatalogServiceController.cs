using Microsoft.AspNetCore.Mvc;
using SalonOS.Api.Authorization;
using SalonOS.Catalog.Application.DTOs;
using SalonOS.Shared.Authorization;

namespace SalonOS.Catalog.API.Controllers;

/// <summary>
/// Catalog (Services) controller.
/// Task 6.4: catalog.* permissions per §R4.
/// catalog.view is public to all authenticated users (SalonManager, Receptionist, Artist, Client).
/// catalog.create/edit/delete/package.manage are SalonManager-only.
/// Authorize on permission strings — never on role names (R2).
/// </summary>
[Route("api/catalog-services")]
[ApiController]
public class CatalogServiceController : ControllerBase
{
    // ── GET /api/catalog-services — catalog.view ──────────────────────────────
    [HttpGet]
    [HasPermission(Permissions.CatalogView)]
    public IActionResult GetCatalogServices()
    {
        return Ok(new List<CatalogServiceDto>());
    }

    // ── GET /api/catalog-services/{id} — catalog.view ────────────────────────
    [HttpGet("{id}")]
    [HasPermission(Permissions.CatalogView)]
    public IActionResult GetCatalogService(Guid id)
    {
        return NotFound(new { message = "Catalog service not found" });
    }

    // ── POST /api/catalog-services — catalog.create ───────────────────────────
    [HttpPost]
    [HasPermission(Permissions.CatalogCreate)]
    public IActionResult CreateCatalogService([FromBody] CreateCatalogServiceDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        return CreatedAtAction(nameof(GetCatalogService), new { id = Guid.NewGuid() }, dto);
    }

    // ── PUT /api/catalog-services/{id} — catalog.edit ────────────────────────
    [HttpPut("{id}")]
    [HasPermission(Permissions.CatalogEdit)]
    public IActionResult UpdateCatalogService(Guid id, [FromBody] UpdateCatalogServiceDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        return Ok(new { message = "Catalog service updated successfully" });
    }

    // ── DELETE /api/catalog-services/{id} — catalog.delete ───────────────────
    [HttpDelete("{id}")]
    [HasPermission(Permissions.CatalogDelete)]
    public IActionResult DeleteCatalogService(Guid id)
    {
        return Ok(new { message = "Catalog service deleted successfully" });
    }

    // ── POST /api/catalog-services/packages — catalog.package.manage ──────────
    [HttpPost("packages")]
    [HasPermission(Permissions.CatalogPackageManage)]
    public IActionResult ManagePackage()
    {
        return Ok(new { message = "Package managed successfully" });
    }
}
