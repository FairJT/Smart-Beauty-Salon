using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SalonOS.Booking.Domain;
using SalonOS.Booking.Infrastructure;
using SalonOS.Identity.Infrastructure;

namespace SalonOS.Api.Controllers;

[Route("api/salons")]
[ApiController]
public class SalonsController : ControllerBase
{
    private readonly IdentityDbContext _identityDb;
    private readonly BookingDbContext _bookingDb;

    public SalonsController(IdentityDbContext identityDb, BookingDbContext bookingDb)
    {
        _identityDb = identityDb;
        _bookingDb = bookingDb;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetSalons([FromQuery] string? search)
    {
        var query = _identityDb.Tenants.Where(t => t.IsActive);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(t => t.Name.Contains(search) || (t.Address != null && t.Address.Contains(search)));

        var tenantList = await query
            .Select(t => new { t.Id, t.Name, t.Slug, t.Description, t.Address, t.Phone, t.LogoUrl })
            .ToListAsync();

        var tenantIds = tenantList.Select(t => t.Id).ToList();
        var ratingStats = await _bookingDb.Bookings
            .Where(b => b.IsRated && b.Rating.HasValue && tenantIds.Contains(b.TenantId))
            .GroupBy(b => b.TenantId)
            .Select(g => new { TenantId = g.Key, AvgRating = g.Average(b => b.Rating!.Value), Count = g.Count() })
            .ToListAsync();

        var ratingMap = ratingStats.ToDictionary(r => r.TenantId);

        var salons = tenantList.Select(t =>
        {
            var stats = ratingMap.GetValueOrDefault(t.Id);
            return new
            {
                slug = t.Slug,
                name = t.Name,
                description = t.Description,
                address = t.Address,
                phoneNumber = t.Phone,
                imageUrl = t.LogoUrl,
                rating = stats?.AvgRating ?? 0,
                reviewCount = stats?.Count ?? 0
            };
        }).ToList();

        return Ok(salons);
    }

    [HttpGet("{slug}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetSalonBySlug(string slug)
    {
        var tenant = await _identityDb.Tenants
            .Where(t => t.Slug == slug && t.IsActive)
            .Select(t => new { t.Id, t.Name, t.Slug, t.Description, t.Address, t.Phone, t.LogoUrl })
            .FirstOrDefaultAsync();

        if (tenant == null)
            return NotFound(new { message = "Salon not found" });

        var ratingStats = await _bookingDb.Bookings
            .Where(b => b.TenantId == tenant.Id && b.IsRated && b.Rating.HasValue)
            .GroupBy(b => b.TenantId)
            .Select(g => new { AvgRating = g.Average(b => b.Rating!.Value), Count = g.Count() })
            .FirstOrDefaultAsync();

        return Ok(new
        {
            slug = tenant.Slug,
            name = tenant.Name,
            description = tenant.Description,
            address = tenant.Address,
            phoneNumber = tenant.Phone,
            imageUrl = tenant.LogoUrl,
            latitude = 0.0,
            longitude = 0.0,
            rating = ratingStats?.AvgRating ?? 0,
            reviewCount = ratingStats?.Count ?? 0
        });
    }
}
