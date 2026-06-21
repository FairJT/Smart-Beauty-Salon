using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using SalonOS.Shared;
using SalonOS.Shared.Authorization;
using SalonOS.Booking.Infrastructure;

namespace SalonOS.Api.Controllers;

[Route("api/salon/insights")]
[ApiController]
[Authorize]
public class ManagerInsightsController : ControllerBase
{
    private readonly BookingDbContext _bookingDb;
    private readonly ITenantContext _tenant;

    public ManagerInsightsController(BookingDbContext bookingDb, ITenantContext tenant)
    {
        _bookingDb = bookingDb;
        _tenant = tenant;
    }

    // Reviews left by customers on bookings
    [HttpGet("reviews")]
    [HasPermission(Permissions.AppointmentViewAll)]
    public async Task<IActionResult> Reviews([FromQuery] Guid? artistId)
    {
        var query = _bookingDb.Bookings
            .Where(b => b.TenantId == _tenant.TenantId && b.IsRated && b.Rating.HasValue);
        if (artistId.HasValue) query = query.Where(b => b.ArtistId == artistId.Value);

        var rows = await query
            .OrderByDescending(b => b.StartsAt)
            .Select(b => new { b.Id, b.ArtistId, b.ClientId, b.Rating, b.Comment, b.StartsAt })
            .ToListAsync();

        return Ok(rows);
    }

    // Distinct customers who have booked at this salon
[HttpGet("customers")]
[HasPermission(Permissions.AppointmentViewAll)]
    public async Task<IActionResult> Customers()
    {
        var rows = await _bookingDb.Bookings
            .Where(b => b.TenantId == _tenant.TenantId)
            .GroupBy(b => b.ClientId)
            .Select(g => new
            {
                ClientId = g.Key,
                Visits = g.Count(),
                LastVisit = g.Max(b => b.StartsAt)
            })
            .OrderByDescending(c => c.LastVisit)
            .ToListAsync();

        return Ok(rows);
    }
}