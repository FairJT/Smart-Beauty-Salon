using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SalonOS.Booking.Domain;
using SalonOS.Booking.Infrastructure;
using SalonOS.Shared;
using SalonOS.Shared.Authorization;

namespace SalonOS.Booking.API.Controllers;

[Route("api/artist-schedules")]
[ApiController]
public class ArtistScheduleController : ControllerBase
{
    private readonly BookingDbContext _db;
    private readonly ITenantContext _tenant;

    public ArtistScheduleController(BookingDbContext db, ITenantContext tenant)
    {
        _db = db;
        _tenant = tenant;
    }

    [HttpGet("by-artist/{artistId}")]
    [HasPermission(Permissions.AppointmentViewAll)]
    public async Task<IActionResult> GetByArtist(Guid artistId)
    {
        var schedules = await _db.ArtistSchedules
            .Where(s => s.ArtistId == artistId && s.TenantId == _tenant.TenantId && !s.IsDeleted)
            .OrderBy(s => s.DayOfWeek)
            .ThenBy(s => s.StartTime)
            .ToListAsync();
        return Ok(schedules);
    }

    [HttpGet("~/api/artist-schedule/my")]
    [HasPermission(Permissions.AppointmentViewOwn)]
    public async Task<IActionResult> GetMySchedule()
    {
        var artistId = User.FindFirst("artist_id")?.Value;
        if (string.IsNullOrEmpty(artistId) || !Guid.TryParse(artistId, out var parsedArtistId))
            return Forbid();

        var now = DateTime.UtcNow;
        var startOfDay = now.Date;
        var endOfDay = startOfDay.AddDays(1);

        var appointments = await _db.Bookings
            .Where(b => b.ArtistId == parsedArtistId && b.TenantId == _tenant.TenantId
                && b.StartsAt >= startOfDay && b.StartsAt < endOfDay
                && b.Status != Booking.Domain.BookingStatus.Cancelled
                && b.Status != Booking.Domain.BookingStatus.NoShow)
            .OrderBy(b => b.StartsAt)
            .Select(b => new
            {
                id = b.Id,
                artistId = b.ArtistId,
                serviceId = b.ServiceId,
                startTime = b.StartsAt,
                endTime = b.EndsAt,
                status = (int)b.Status,
                estimatedPrice = (double)b.EstimatedPrice.Amount,
                depositAmount = (double)b.DepositAmount.Amount,
                isRated = b.IsRated,
                rating = (int?)b.Rating,
                comment = b.Comment ?? "",
                salonName = "",
                artistName = "",
                serviceName = "",
                clientName = b.ClientId ?? ""
            })
            .ToListAsync();

        return Ok(appointments);
    }

    [HttpGet("~/api/artist-schedule/my/stats")]
    [HasPermission(Permissions.AppointmentViewOwn)]
    public async Task<IActionResult> GetMyStats()
    {
        var artistIdClaim = User.FindFirst("artist_id")?.Value;
        if (string.IsNullOrEmpty(artistIdClaim) || !Guid.TryParse(artistIdClaim, out var parsedArtistId))
            return Forbid();

        var now = DateTime.UtcNow;
        var startOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var endOfMonth = startOfMonth.AddMonths(1);

        var todayStart = now.Date;
        var todayEnd = todayStart.AddDays(1);

        var todayCount = await _db.Bookings
            .CountAsync(b => b.ArtistId == parsedArtistId && b.TenantId == _tenant.TenantId
                && b.StartsAt >= todayStart && b.StartsAt < todayEnd
                && b.Status != Booking.Domain.BookingStatus.Cancelled);

        var monthCount = await _db.Bookings
            .CountAsync(b => b.ArtistId == parsedArtistId && b.TenantId == _tenant.TenantId
                && b.StartsAt >= startOfMonth && b.StartsAt < endOfMonth
                && b.Status != Booking.Domain.BookingStatus.Cancelled);

        return Ok(new { todayAppointments = todayCount, monthAppointments = monthCount });
    }

    [HttpPost]
    [HasPermission(Permissions.SalonEdit)]
    public async Task<IActionResult> Create([FromBody] CreateArtistScheduleDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var schedule = new ArtistSchedule
        {
            ArtistId = dto.ArtistId,
            DayOfWeek = dto.DayOfWeek,
            StartTime = dto.StartTime,
            EndTime = dto.EndTime,
            TenantId = _tenant.TenantId
        };

        _db.ArtistSchedules.Add(schedule);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetByArtist), new { artistId = schedule.ArtistId }, schedule);
    }

    [HttpPut("{id}")]
    [HasPermission(Permissions.SalonEdit)]
    public async Task<IActionResult> Update(Guid id, [FromBody] CreateArtistScheduleDto dto)
    {
        var schedule = await _db.ArtistSchedules
            .FirstOrDefaultAsync(s => s.Id == id && s.TenantId == _tenant.TenantId);
        if (schedule == null) return NotFound();

        schedule.DayOfWeek = dto.DayOfWeek;
        schedule.StartTime = dto.StartTime;
        schedule.EndTime = dto.EndTime;
        schedule.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return Ok(schedule);
    }

    [HttpDelete("{id}")]
    [HasPermission(Permissions.SalonEdit)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var schedule = await _db.ArtistSchedules
            .FirstOrDefaultAsync(s => s.Id == id && s.TenantId == _tenant.TenantId);
        if (schedule == null) return NotFound();

        schedule.IsDeleted = true;
        schedule.DeletedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return NoContent();
    }
}

public class CreateArtistScheduleDto
{
    public Guid ArtistId { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
}
