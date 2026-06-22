# A4 — Customer check-in + reschedule request 🟡

Check-in is a small field on `Booking`. Reschedule is a REQUEST (manager approves), not direct power.

---

## Part 1 — check-in field on Booking

### Step 1 — add the field
**File:** `src/Modules/Booking/Domain/Booking.cs`
**Find (exact):**
```csharp
    public BookingStatus Status { get; set; } = BookingStatus.Pending;
```
**Replace with:**
```csharp
    public BookingStatus Status { get; set; } = BookingStatus.Pending;
    public DateTime? CheckedInAt { get; set; }   // set when the client arrives (item 6/7)
```

### Step 2 — migration (Booking context)
```powershell
dotnet ef migrations add BookingCheckIn --project src\Modules\Booking --startup-project src\SalonOS.Api --context BookingDbContext
```

---

## Part 2 — reschedule request entity (AppDbContext)

### Step 3 — entity
**New file:** `src/SalonOS.Infrastructure/SalonMgmt/RescheduleRequest.cs`
```csharp
using SalonOS.Shared;

namespace SalonOS.Infrastructure;

public enum RescheduleStatus { Pending = 1, Approved = 2, Rejected = 3 }

public class RescheduleRequest : TenantEntity
{
    public Guid BookingId { get; set; }
    public Guid ArtistId { get; set; }
    public DateTime ProposedStart { get; set; }
    public string? Reason { get; set; }
    public RescheduleStatus Status { get; set; } = RescheduleStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
```

### Step 4 — register DbSet
**File:** `src/SalonOS.Infrastructure/AppDbContext.cs`
**Find (exact):**
```csharp
    public DbSet<OutboxMessage> OutboxMessages { get; set; }
```
**Replace with:**
```csharp
    public DbSet<OutboxMessage> OutboxMessages { get; set; }
    public DbSet<RescheduleRequest> RescheduleRequests { get; set; }
```

### Step 5 — migration (AppDbContext)
```powershell
dotnet ef migrations add RescheduleRequests --project src\SalonOS.Infrastructure --startup-project src\SalonOS.Api --context AppDbContext
```

### Step 6 — controllers (check-in + reschedule, one file)
**New file:** `src/SalonOS.Api/Controllers/ArtistVisitController.cs`
```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using SalonOS.Shared;
using SalonOS.Shared.Authorization;
using SalonOS.Infrastructure;
using SalonOS.Booking.Infrastructure;

namespace SalonOS.Api.Controllers;

[Route("api/artist-visit")]
[ApiController]
[Authorize]
public class ArtistVisitController : ControllerBase
{
    private readonly AppDbContext _app;
    private readonly BookingDbContext _booking;
    private readonly ITenantContext _tenant;
    public ArtistVisitController(AppDbContext app, BookingDbContext booking, ITenantContext tenant)
    {
        _app = app; _booking = booking; _tenant = tenant;
    }

    private bool TryArtist(out Guid artistId)
    {
        artistId = Guid.Empty;
        var v = User.FindFirst("artist_id")?.Value;
        return !string.IsNullOrEmpty(v) && Guid.TryParse(v, out artistId);
    }

    // Check the client in.
    [HttpPut("{bookingId}/check-in")]
    [HasPermission(Permissions.AppointmentCheckIn)]
    public async Task<IActionResult> CheckIn(Guid bookingId)
    {
        if (!TryArtist(out var artistId)) return Forbid();
        var b = await _booking.Bookings
            .FirstOrDefaultAsync(x => x.Id == bookingId && x.TenantId == _tenant.TenantId && x.ArtistId == artistId);
        if (b is null) return NotFound();
        b.CheckedInAt = DateTime.UtcNow;
        b.Status = Booking.Domain.BookingStatus.InProgress;
        await _booking.SaveChangesAsync();
        return Ok(new { checkedInAt = b.CheckedInAt });
    }

    // Request to move an appointment — goes to the manager as Pending.
    public record RescheduleReq(Guid BookingId, DateTime ProposedStart, string? Reason);

    [HttpPost("reschedule-request")]
    [HasPermission(Permissions.RescheduleRequestCreate)]
    public async Task<IActionResult> RequestReschedule([FromBody] RescheduleReq r)
    {
        if (!TryArtist(out var artistId)) return Forbid();
        var rr = new RescheduleRequest { BookingId = r.BookingId, ArtistId = artistId, ProposedStart = r.ProposedStart, Reason = r.Reason };
        _app.RescheduleRequests.Add(rr);
        await _app.SaveChangesAsync();
        return Ok(rr);
    }

    // Manager reviews reschedule requests.
    [HttpGet("reschedule-requests")]
    [HasPermission(Permissions.AppointmentViewAll)]
    public async Task<IActionResult> ListReschedules([FromQuery] RescheduleStatus? status)
    {
        var q = _app.RescheduleRequests.AsQueryable();
        if (status.HasValue) q = q.Where(x => x.Status == status.Value);
        return Ok(await q.OrderByDescending(x => x.CreatedAt).ToListAsync());
    }

    [HttpPut("reschedule-requests/{id}/decision")]
    [HasPermission(Permissions.AppointmentCancelAll)]
    public async Task<IActionResult> Decide(Guid id, [FromQuery] bool approve)
    {
        var rr = await _app.RescheduleRequests.FirstOrDefaultAsync(x => x.Id == id);
        if (rr is null) return NotFound();
        rr.Status = approve ? RescheduleStatus.Approved : RescheduleStatus.Rejected;
        rr.UpdatedAt = DateTime.UtcNow;
        await _app.SaveChangesAsync();
        return Ok(rr);
        // NOTE: actually moving the booking time on approval is a follow-up (Claude) — this records the decision.
    }
}
```

**Done when:** build succeeds; artist can check a client in and file a reschedule request; manager can list/decide.

**⚠️ Review:** approving a reschedule should also move the booking's `StartsAt`/`EndsAt` (respecting the
double-booking rule) — that wiring is a Claude task; this only records the decision.
