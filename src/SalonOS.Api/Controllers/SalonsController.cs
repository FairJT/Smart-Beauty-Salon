using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SalonOS.Identity.Infrastructure;

namespace SalonOS.Api.Controllers;

[Route("api/salons")]
[ApiController]
public class SalonsController : ControllerBase
{
    private readonly IdentityDbContext _db;

    public SalonsController(IdentityDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetSalons([FromQuery] string? search)
    {
        var query = _db.Tenants.Where(t => t.IsActive);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(t => t.Name.Contains(search) || (t.Address != null && t.Address.Contains(search)));

        var salons = await query
            .Select(t => new
            {
                id = t.SalonId,
                name = t.Name,
                slug = t.Slug,
                description = t.Description,
                address = t.Address,
                phoneNumber = t.Phone,
                imageUrl = t.LogoUrl,
                rating = 0,
                reviewCount = 0
            })
            .ToListAsync();

        return Ok(salons);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetSalon(int id)
    {
        var tenant = await _db.Tenants
            .Where(t => t.SalonId == id && t.IsActive)
            .Select(t => new
            {
                id = t.SalonId,
                name = t.Name,
                slug = t.Slug,
                description = t.Description,
                address = t.Address,
                phoneNumber = t.Phone,
                imageUrl = t.LogoUrl,
                latitude = 0.0,
                longitude = 0.0,
                rating = 0,
                reviewCount = 0
            })
            .FirstOrDefaultAsync();

        if (tenant == null)
            return NotFound(new { message = "Salon not found" });

        return Ok(tenant);
    }
}
