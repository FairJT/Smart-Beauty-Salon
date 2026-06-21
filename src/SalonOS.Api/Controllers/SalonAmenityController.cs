using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using SalonOS.Shared.Authorization;
using SalonOS.Infrastructure;

namespace SalonOS.Api.Controllers;

[Route("api/salon/amenities")]
[ApiController]
[Authorize]
public class SalonAmenityController : ControllerBase
{
    private readonly AppDbContext _db;
    public SalonAmenityController(AppDbContext db) => _db = db;

    public record AmenityRequest(string Name, string? Icon);

    [HttpGet]
    [HasPermission(Permissions.SalonView)]
    public async Task<IActionResult> List() =>
        Ok(await _db.SalonAmenities.OrderBy(a => a.Name).ToListAsync());

    [HttpPost]
    [HasPermission(Permissions.SalonSettingsManage)]
    public async Task<IActionResult> Create([FromBody] AmenityRequest r)
    {
        var a = new SalonAmenity { Name = r.Name, Icon = r.Icon };
        _db.SalonAmenities.Add(a);
        await _db.SaveChangesAsync();
        return Ok(a);
    }

    [HttpDelete("{id}")]
    [HasPermission(Permissions.SalonSettingsManage)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var a = await _db.SalonAmenities.FirstOrDefaultAsync(x => x.Id == id);
        if (a is null) return NotFound();
        a.IsDeleted = true;
        a.DeletedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Ok(new { deleted = true });
    }
}