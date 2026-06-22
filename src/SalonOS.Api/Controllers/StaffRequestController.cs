using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using SalonOS.Shared.Authorization;
using SalonOS.Infrastructure;
using SalonOS.Shared;

namespace SalonOS.Api.Controllers;

[Route("api/staff-requests")]
[ApiController]
[Authorize]
public class StaffRequestController : ControllerBase
{
    private readonly AppDbContext _db;
    public StaffRequestController(AppDbContext db) => _db = db;

    public record CreateReq(StaffRequestType Type, string Title, string? Detail);

    private bool TryArtist(out Guid artistId)
    {
        artistId = Guid.Empty;
        var v = User.FindFirst("artist_id")?.Value;
        return !string.IsNullOrEmpty(v) && Guid.TryParse(v, out artistId);
    }

    // Artist: create request
    [HttpPost]
    [HasPermission(Permissions.StaffRequestCreate)]
    public async Task<IActionResult> Create([FromBody] CreateReq r)
    {
        if (!TryArtist(out var artistId)) return Forbid();
        var sr = new StaffRequest
        {
            ArtistId = artistId,
            Type = r.Type,
            Title = r.Title,
            Detail = r.Detail
        };
        _db.StaffRequests.Add(sr);
        await _db.SaveChangesAsync();
        return Ok(sr);
    }

    // Artist: list own requests
    [HttpGet("mine")]
    [HasPermission(Permissions.StaffRequestCreate)]
    public async Task<IActionResult> Mine()
    {
        if (!TryArtist(out var artistId)) return Forbid();
        var list = await _db.StaffRequests
            .Where(s => s.ArtistId == artistId)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();
        return Ok(list);
    }

    // Manager: list all requests, optional filter by status
    [HttpGet]
    [HasPermission(Permissions.SalonView)]
    public async Task<IActionResult> All([FromQuery] StaffRequestStatus? status)
    {
        var q = _db.StaffRequests.AsQueryable();
        if (status.HasValue) q = q.Where(s => s.Status == status.Value);
        var list = await q.OrderByDescending(s => s.CreatedAt).ToListAsync();
        return Ok(list);
    }

    // Manager: update status
    [HttpPut("{id}/status")]
    [HasPermission(Permissions.SalonSettingsManage)]
    public async Task<IActionResult> SetStatus(Guid id, [FromQuery] StaffRequestStatus status)
    {
        var sr = await _db.StaffRequests.FirstOrDefaultAsync(x => x.Id == id);
        if (sr == null) return NotFound();
        sr.Status = status;
        sr.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Ok(sr);
    }
}