using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SalonOS.Catalog.Infrastructure;
using SalonOS.Identity.Infrastructure;

namespace SalonOS.Api.Controllers;

[Route("api/services")]
[ApiController]
public class ServicesController : ControllerBase
{
    private readonly CatalogDbContext _catalogDb;
    private readonly IdentityDbContext _identityDb;

    public ServicesController(CatalogDbContext catalogDb, IdentityDbContext identityDb)
    {
        _catalogDb = catalogDb;
        _identityDb = identityDb;
    }

    [HttpGet("salon/{slug}")]
    public async Task<IActionResult> GetServicesBySalon(string slug)
    {
        var tenant = await _identityDb.Tenants
            .Where(t => t.Slug == slug && t.IsActive)
            .Select(t => t.Id)
            .FirstOrDefaultAsync();

        if (tenant == Guid.Empty)
            return NotFound(new { message = "Salon not found" });

        var services = await _catalogDb.CatalogServices
            .Where(s => s.TenantId == tenant && s.IsActive && !s.IsDeleted)
            .Select(s => new
            {
                id = s.Id,
                name = s.Name,
                description = s.Description,
                price = (double)s.BasePrice.Amount,
                durationMinutes = s.BaseDurationMinutes,
                imageUrl = (string?)null,
                isActive = s.IsActive,
                templateId = (int?)null
            })
            .ToListAsync();

        return Ok(services);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetService(Guid id)
    {
        var service = await _catalogDb.CatalogServices
            .Where(s => s.Id == id && !s.IsDeleted)
            .Select(s => new
            {
                id = s.Id,
                name = s.Name,
                description = s.Description,
                price = (double)s.BasePrice.Amount,
                durationMinutes = s.BaseDurationMinutes,
                imageUrl = (string?)null,
                isActive = s.IsActive,
                templateId = (int?)null
            })
            .FirstOrDefaultAsync();

        if (service == null)
            return NotFound(new { message = "Service not found" });

        return Ok(service);
    }
}
