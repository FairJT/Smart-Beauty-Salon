using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using SalonOS.Shared.Authorization;
using SalonOS.Infrastructure;

namespace SalonOS.Api.Controllers;

[Route("api/salon")]
[ApiController]
[Authorize]
public class WorkingHoursController : ControllerBase
{
    private readonly AppDbContext _db;
    public WorkingHoursController(AppDbContext db) => _db = db;

    public record HourRequest(int DayOfWeek, string OpenTime, string CloseTime, bool IsClosed);
    public record ClosureRequest(DateTime Date, bool IsClosed, string? Reason);

    // ── Weekly hours ──────────────────────────────
    [HttpGet("working-hours")]
    [HasPermission(Permissions.SalonView)]
    public async Task<IActionResult> GetHours() =>
        Ok(await _db.WorkingHours.OrderBy(h => h.DayOfWeek).ToListAsync());

    [HttpPut("working-hours")]
    [HasPermission(Permissions.SalonSettingsManage)]
    public async Task<IActionResult> SetHours([FromBody] List<HourRequest> rows)
    {
        var existing = await _db.WorkingHours.ToListAsync();
        _db.WorkingHours.RemoveRange(existing);                  // replace the whole week
        foreach (var r in rows)
            _db.WorkingHours.Add(new WorkingHour { DayOfWeek = r.DayOfWeek, OpenTime = r.OpenTime, CloseTime = r.CloseTime, IsClosed = r.IsClosed });
        await _db.SaveChangesAsync();
        return Ok(new { saved = rows.Count });
    }

    // ── Date closures ─────────────────────────────
    [HttpGet("closures")]
    [HasPermission(Permissions.SalonView)]
    public async Task<IActionResult> GetClosures() =>
        Ok(await _db.SalonClosures.OrderBy(c => c.Date).ToListAsync());

    [HttpPost("closures")]
    [HasPermission(Permissions.SalonSettingsManage)]
    public async Task<IActionResult> AddClosure([FromBody] ClosureRequest r)
    {
        var c = new SalonClosure { Date = r.Date.Date, IsClosed = r.IsClosed, Reason = r.Reason };
        _db.SalonClosures.Add(c);
        await _db.SaveChangesAsync();
        return Ok(c);
    }

    [HttpDelete("closures/{id}")]
    [HasPermission(Permissions.SalonSettingsManage)]
    public async Task<IActionResult> DeleteClosure(Guid id)
    {
        var c = await _db.SalonClosures.FirstOrDefaultAsync(x => x.Id == id);
        if (c is null) return NotFound();
        c.IsDeleted = true;
        c.DeletedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Ok(new { deleted = true });
    }
}