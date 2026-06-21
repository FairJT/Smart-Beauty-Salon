using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using SalonOS.Shared.Authorization;
using SalonOS.Infrastructure;

namespace SalonOS.Api.Controllers;

[Route("api/salon/notices")]
[ApiController]
[Authorize]
public class SalonNoticeController : ControllerBase
{
    private readonly AppDbContext _db;
    public SalonNoticeController(AppDbContext db) => _db = db;

    public record NoticeRequest(string Title, string Body, bool IsPinned, DateTime? StartsAt, DateTime? EndsAt);

    [HttpGet]
    [HasPermission(Permissions.SalonView)]
    public async Task<IActionResult> List() =>
        Ok(await _db.SalonNotices
            .OrderByDescending(n => n.IsPinned)
            .ThenByDescending(n => n.CreatedAt)
            .ToListAsync());

    [HttpPost]
    [HasPermission(Permissions.SalonSettingsManage)]
    public async Task<IActionResult> Create([FromBody] NoticeRequest r)
    {
        var n = new SalonNotice
        {
            Title = r.Title,
            Body = r.Body,
            IsPinned = r.IsPinned,
            StartsAt = r.StartsAt,
            EndsAt = r.EndsAt
        };
        _db.SalonNotices.Add(n);
        await _db.SaveChangesAsync();
        return Ok(n);
    }

    [HttpPut("{id}")]
    [HasPermission(Permissions.SalonSettingsManage)]
    public async Task<IActionResult> Update(Guid id, [FromBody] NoticeRequest r)
    {
        var n = await _db.SalonNotices.FirstOrDefaultAsync(x => x.Id == id);
        if (n is null) return NotFound();
        n.Title = r.Title;
        n.Body = r.Body;
        n.IsPinned = r.IsPinned;
        n.StartsAt = r.StartsAt;
        n.EndsAt = r.EndsAt;
        n.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Ok(n);
    }

    [HttpDelete("{id}")]
    [HasPermission(Permissions.SalonSettingsManage)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var n = await _db.SalonNotices.FirstOrDefaultAsync(x => x.Id == id);
        if (n is null) return NotFound();
        n.IsDeleted = true;
        n.DeletedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Ok(new { deleted = true });
    }
}