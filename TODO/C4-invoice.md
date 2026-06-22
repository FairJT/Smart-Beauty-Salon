# C4 — View invoice (item 12, derived) 🟢

A simple invoice derived from the client's own booking (no new table). A full invoice tied to a real
payment is a Claude task; this gives the client a readable summary now.

## Step 1 — controller
**New file:** `src/SalonOS.Api/Controllers/InvoiceController.cs`
```csharp
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using SalonOS.Shared;
using SalonOS.Shared.Authorization;
using SalonOS.Booking.Infrastructure;

namespace SalonOS.Api.Controllers;

[Route("api/invoices")]
[ApiController]
[Authorize]
public class InvoiceController : ControllerBase
{
    private readonly BookingDbContext _booking;
    private readonly ITenantContext _tenant;
    public InvoiceController(BookingDbContext booking, ITenantContext tenant)
    {
        _booking = booking; _tenant = tenant;
    }

    // Invoice for one of MY bookings.
    [HttpGet("{bookingId}")]
    [HasPermission(Permissions.AppointmentViewOwn)]
    public async Task<IActionResult> Get(Guid bookingId)
    {
        var me = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var b = await _booking.Bookings
            .FirstOrDefaultAsync(x => x.Id == bookingId && x.TenantId == _tenant.TenantId && x.ClientId == me);
        if (b is null) return NotFound();

        return Ok(new
        {
            bookingId   = b.Id,
            serviceId   = b.ServiceId,
            artistId    = b.ArtistId,
            startsAt    = b.StartsAt,
            status      = b.Status.ToString(),
            estimated   = new { amount = b.EstimatedPrice.Amount, currency = b.EstimatedPrice.Currency },
            deposit     = b.DepositAmount == null ? null : new { amount = b.DepositAmount.Amount, currency = b.DepositAmount.Currency },
            final       = b.FinalPrice == null ? null : new { amount = b.FinalPrice.Amount, currency = b.FinalPrice.Currency }
        });
    }
}
```

**⚠️ If a field name doesn't match** (e.g. `EstimatedPrice` / `DepositAmount` / `FinalPrice` / `ServiceId`),
STOP and report the real `Booking` property names — don't guess.

**Done when:** build succeeds; a client can `GET /api/invoices/{theirBookingId}` and see the summary.
