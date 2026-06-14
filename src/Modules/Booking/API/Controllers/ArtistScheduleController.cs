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
