using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SalonOS.Booking.Domain;
using SalonOS.Booking.Infrastructure;
using SalonOS.Shared;
using SalonOS.Shared.Authorization;

namespace SalonOS.Booking.API.Controllers;

[Route("api/leaves")]
[ApiController]
public class LeaveController : ControllerBase
{
    private readonly BookingDbContext _db;
    private readonly ITenantContext _tenant;

    public LeaveController(BookingDbContext db, ITenantContext tenant)
    {
        _db = db;
        _tenant = tenant;
    }

    [HttpGet("by-artist/{artistId}")]
    [HasPermission(Permissions.AppointmentViewAll)]
    public async Task<IActionResult> GetByArtist(Guid artistId)
    {
        var leaves = await _db.Leaves
            .Where(l => l.ArtistId == artistId && l.TenantId == _tenant.TenantId && !l.IsDeleted)
            .OrderBy(l => l.StartDateTime)
            .ToListAsync();
        return Ok(leaves);
    }

    [HttpGet]
    [HasPermission(Permissions.AppointmentViewAll)]
    public async Task<IActionResult> GetAll()
    {
        var leaves = await _db.Leaves
            .Where(l => l.TenantId == _tenant.TenantId && !l.IsDeleted)
            .OrderBy(l => l.StartDateTime)
            .ToListAsync();
        return Ok(leaves);
    }

    [HttpPost]
    [HasPermission(Permissions.SalonEdit)]
    public async Task<IActionResult> Create([FromBody] CreateLeaveDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var leave = new Leave
        {
            ArtistId = dto.ArtistId,
            StartDateTime = dto.StartDateTime,
            EndDateTime = dto.EndDateTime,
            Reason = dto.Reason,
            Status = LeaveStatus.Approved,
            TenantId = _tenant.TenantId
        };

        _db.Leaves.Add(leave);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetByArtist), new { artistId = leave.ArtistId }, leave);
    }

    [HttpPut("{id}/status")]
    [HasPermission(Permissions.SalonEdit)]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateLeaveStatusDto dto)
    {
        var leave = await _db.Leaves
            .FirstOrDefaultAsync(l => l.Id == id && l.TenantId == _tenant.TenantId);
        if (leave == null) return NotFound();

        leave.Status = dto.Status;
        leave.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Ok(leave);
    }

    [HttpDelete("{id}")]
    [HasPermission(Permissions.SalonEdit)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var leave = await _db.Leaves
            .FirstOrDefaultAsync(l => l.Id == id && l.TenantId == _tenant.TenantId);
        if (leave == null) return NotFound();

        leave.IsDeleted = true;
        leave.DeletedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return NoContent();
    }
}

public class CreateLeaveDto
{
    public Guid ArtistId { get; set; }
    public DateTime StartDateTime { get; set; }
    public DateTime EndDateTime { get; set; }
    public string? Reason { get; set; }
}

public class UpdateLeaveStatusDto
{
    public LeaveStatus Status { get; set; }
}
