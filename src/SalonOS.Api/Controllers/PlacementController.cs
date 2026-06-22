using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using SalonOS.Shared.Authorization;
using SalonOS.Infrastructure;

namespace SalonOS.Api.Controllers;

[Route("api/placements")]
[ApiController]
public class PlacementController : ControllerBase
{
    private readonly AppDbContext _db;
    public PlacementController(AppDbContext db) => _db = db;

    public record PlacementReq(Guid SalonTenantId, PlacementType Type, DateTime StartsAt, DateTime EndsAt, int Weight);

    // Public: currently-active promoted salons (homepage uses this to order featured salons).
    [HttpGet("active")]
    [AllowAnonymous]
    public async Task<IActionResult> Active([FromQuery] PlacementType? type)
    {
        var now = DateTime.UtcNow;
        var query = _db.SalonPlacements.Where(p => p.IsActive && p.StartsAt <= now && p.EndsAt >= now);
        if (type.HasValue) query = query.Where(p => p.Type == type.Value);
        return Ok(await query.OrderByDescending(p => p.Weight).ToListAsync());
    }

    [HttpPost]
    [Authorize]
    [HasPermission(Permissions.PlatformConfigManage)]
    public async Task<IActionResult> Create([FromBody] PlacementReq r)
    {
        var placement = new SalonPlacement
        {
            SalonTenantId = r.SalonTenantId,
            Type = r.Type,
            StartsAt = r.StartsAt,
            EndsAt = r.EndsAt,
            Weight = r.Weight
        };
        _db.SalonPlacements.Add(placement);
        await _db.SaveChangesAsync();
        return Ok(placement);
    }

    [HttpDelete("{id}")]
    [Authorize]
    [HasPermission(Permissions.PlatformConfigManage)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var placement = await _db.SalonPlacements.FindAsync(id);
        if (placement is null) return NotFound();
        placement.IsActive = false;
        await _db.SaveChangesAsync();
        return Ok(new { deactivated = true });
    }
}