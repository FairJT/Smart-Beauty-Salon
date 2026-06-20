using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SalonOS.Booking.Domain;
using SalonOS.Booking.Infrastructure;
using SalonOS.Identity.Infrastructure;
using SalonOS.Shared;

namespace SalonOS.Api.Controllers;

[Route("api/salons")]
[ApiController]
public class SalonsController : ControllerBase
{
    private readonly IdentityDbContext _identityDb;
    private readonly BookingDbContext _bookingDb;
    private readonly IBookingService _bookings;
    private readonly ITenantContext _tenant;

    public SalonsController(
        IdentityDbContext identityDb,
        BookingDbContext bookingDb,
        IBookingService bookings,
        ITenantContext tenant)
    {
        _identityDb = identityDb;
        _bookingDb = bookingDb;
        _bookings = bookings;
        _tenant = tenant;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetSalons([FromQuery] string? search)
    {
        var query = _identityDb.Tenants.Where(t => t.IsActive);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(t => t.Name.Contains(search) || (t.Address != null && t.Address.Contains(search)));

        var salons = await query
            .Select(t => new
            {
                slug = t.Slug,
                name = t.Name,
                description = t.Description,
                address = t.Address,
                phoneNumber = t.Phone,
                imageUrl = t.LogoUrl,
                rating = t.RatingCount > 0 ? (double)t.RatingSum / t.RatingCount : 0,
                reviewCount = t.RatingCount
            })
            .ToListAsync();

        return Ok(salons);
    }

    // Public availability for one of a salon's artists. Resolves the tenant from the
    // PUBLIC slug, then scopes the request so RLS allows that salon's schedule/bookings.
    [HttpGet("{slug}/slots")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPublicSlots(
        string slug,
        [FromQuery] string artistId,
        [FromQuery] DateTime date,
        [FromQuery] int durationMinutes = 30)
    {
        if (!Guid.TryParse(artistId, out var parsedArtistId))
            return BadRequest(new { message = "Invalid artistId" });

        var tenant = await _identityDb.Tenants
            .Where(t => t.Slug == slug && t.IsActive)
            .Select(t => new { t.Id })
            .FirstOrDefaultAsync();

        if (tenant == null)
            return NotFound(new { message = "Salon not found" });

        // Scope this anonymous request to the resolved salon (read-only).
        _tenant.SetPublicTenant(tenant.Id);

        var slots = await _bookings.GetAvailableSlotsAsync(
            parsedArtistId, date, durationMinutes, tenant.Id);
        return Ok(slots);
    }

    [HttpGet("{slug}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetSalonBySlug(string slug)
    {
        var tenant = await _identityDb.Tenants
            .Where(t => t.Slug == slug && t.IsActive)
            .Select(t => new { t.Id, t.Name, t.Slug, t.Description, t.Address, t.Phone, t.LogoUrl, t.RatingSum, t.RatingCount })
            .FirstOrDefaultAsync();

        if (tenant == null)
            return NotFound(new { message = "Salon not found" });

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
            rating = tenant.RatingCount > 0 ? (double)tenant.RatingSum / tenant.RatingCount : 0,
            reviewCount = tenant.RatingCount
        });
    }
}
