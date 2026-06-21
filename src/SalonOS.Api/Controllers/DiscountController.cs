using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using SalonOS.Shared.Authorization;
using SalonOS.Infrastructure;

namespace SalonOS.Api.Controllers;

[Route("api/salon/discounts")]
[ApiController]
[Authorize]
public class DiscountController : ControllerBase
{
    private readonly AppDbContext _db;
    public DiscountController(AppDbContext db) => _db = db;

    public record DiscountRequest(string? Code, int? Percent, long? AmountMinor,
        DateTime StartsAt, DateTime EndsAt, string? TargetClientId, int? MaxUses);

    [HttpGet]
    [HasPermission(Permissions.SalonView)]
    public async Task<IActionResult> List() =>
        Ok(await _db.Discounts
            .Where(d => d.IsActive)
            .OrderByDescending(d => d.StartsAt)
            .ToListAsync());

    [HttpPost]
    [HasPermission(Permissions.SalonSettingsManage)]
    public async Task<IActionResult> Create([FromBody] DiscountRequest r)
    {
        if (r.Percent is null && r.AmountMinor is null)
            return BadRequest(new { message = "Provide either Percent or AmountMinor" });

        var d = new Discount
        {
            Code = r.Code,
            Percent = r.Percent,
            AmountMinor = r.AmountMinor,
            StartsAt = r.StartsAt,
            EndsAt = r.EndsAt,
            TargetClientId = r.TargetClientId,
            MaxUses = r.MaxUses
        };
        _db.Discounts.Add(d);
        await _db.SaveChangesAsync();
        return Ok(d);
    }

    [HttpDelete("{id}")]
    [HasPermission(Permissions.SalonSettingsManage)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var d = await _db.Discounts.FirstOrDefaultAsync(x => x.Id == id);
        if (d is null) return NotFound();
        d.IsActive = false;
        d.IsDeleted = true;
        d.DeletedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Ok(new { deleted = true });
    }
}