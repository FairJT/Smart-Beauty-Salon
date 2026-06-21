# M8 — Manager view: reviews + customer list 🟢

Read-only endpoints for the manager. These read Booking data, so the controller injects
`BookingDbContext` + `ITenantContext` and filters by the current tenant (same pattern as `SalonsController`).

## Step 1 — controller
**New file:** `src/SalonOS.Api/Controllers/ManagerInsightsController.cs`
```csharp
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

    // Reviews customers left on this salon's bookings (salon + artist feedback).
    [HttpGet("reviews")]
    [HasPermission(Permissions.AppointmentViewAll)]
    public async Task<IActionResult> Reviews([FromQuery] Guid? artistId)
    {
        var q = _bookingDb.Bookings
            .Where(b => b.TenantId == _tenant.TenantId && b.IsRated && b.Rating.HasValue);
        if (artistId.HasValue) q = q.Where(b => b.ArtistId == artistId.Value);

        var rows = await q
            .OrderByDescending(b => b.StartsAt)
            .Select(b => new { b.Id, b.ArtistId, b.ClientId, b.Rating, b.Comment, b.StartsAt })
            .ToListAsync();
        return Ok(rows);
    }

    // Distinct customers who have booked at this salon.
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
                LastVisit = g.Max(x => x.StartsAt)
            })
            .OrderByDescending(c => c.LastVisit)
            .ToListAsync();
        return Ok(rows);
    }
}
```

**Done when:** build succeeds; `GET /api/salon/insights/reviews` and `.../customers` return data for a manager.

**Note:** `BookingDbContext` doesn't auto-filter by tenant, so the manual `b.TenantId == _tenant.TenantId`
above is REQUIRED — don't remove it.
