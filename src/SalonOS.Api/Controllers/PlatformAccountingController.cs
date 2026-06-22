using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using SalonOS.Shared.Authorization;
using SalonOS.Infrastructure;

namespace SalonOS.Api.Controllers;

[Route("api/admin/accounting")]
[ApiController]
[Authorize]
[HasPermission(Permissions.ReportPlatformView)]
public class PlatformAccountingController : ControllerBase
{
    private readonly AppDbContext _db;
    public PlatformAccountingController(AppDbContext db) => _db = db;

    // High-level platform revenue snapshot.
    [HttpGet("overview")]
    public async Task<IActionResult> Overview()
    {
        var now = DateTime.UtcNow;

        var activePlacements = await _db.SalonPlacements
            .CountAsync(p => p.IsActive && p.StartsAt <= now && p.EndsAt >= now);

        var placementsByType = await _db.SalonPlacements
            .Where(p => p.IsActive)
            .GroupBy(p => p.Type)
            .Select(g => new { type = g.Key.ToString(), count = g.Count() })
            .ToListAsync();

        return Ok(new
        {
            generatedAt = now,
            activePlacements,
            placementsByType
            // Panel-sale revenue (SalonPackageLicense) can be added here once its price field is confirmed.
        });
    }
}