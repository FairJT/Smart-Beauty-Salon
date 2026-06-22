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
        _app = app;
        _booking = booking;
        _tenant = tenant;
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
        var rr = new RescheduleRequest
        {
            BookingId = r.BookingId,
            ArtistId = artistId,
            ProposedStart = r.ProposedStart,
            Reason = r.Reason
        };
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
    }
}