using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using SalonOS.Shared.Authorization;
using SalonOS.Infrastructure;

namespace SalonOS.Api.Controllers;

[Route("api/client-feedback")]
[ApiController]
[Authorize]
public class ClientFeedbackController : ControllerBase
{
    private readonly AppDbContext _db;
    public ClientFeedbackController(AppDbContext db) => _db = db;

    public record FeedbackRequest(ClientFeedbackType Type, string Title, string? Detail);

    private string? Me() => User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    // Client: submit + see own
    [HttpPost]
    [HasPermission(Permissions.ClientFeedbackCreate)]
    public async Task<IActionResult> Create([FromBody] FeedbackRequest r)
    {
        var me = Me();
        if (string.IsNullOrEmpty(me)) return Forbid();
        var f = new ClientFeedback { ClientId = me, Type = r.Type, Title = r.Title, Detail = r.Detail };
        _db.ClientFeedbacks.Add(f);
        await _db.SaveChangesAsync();
        return Ok(f);
    }

    [HttpGet("mine")]
    [HasPermission(Permissions.ClientFeedbackCreate)]
    public async Task<IActionResult> Mine()
    {
        var me = Me();
        if (string.IsNullOrEmpty(me)) return Forbid();
        return Ok(await _db.ClientFeedbacks.Where(f => f.ClientId == me).OrderByDescending(f => f.CreatedAt).ToListAsync());
    }

    // Manager: list all for the salon + resolve
    [HttpGet]
    [HasPermission(Permissions.SalonView)]
    public async Task<IActionResult> All([FromQuery] ClientFeedbackStatus? status)
    {
        var q = _db.ClientFeedbacks.AsQueryable();
        if (status.HasValue) q = q.Where(f => f.Status == status.Value);
        return Ok(await q.OrderByDescending(f => f.CreatedAt).ToListAsync());
    }

    [HttpPut("{id}/status")]
    [HasPermission(Permissions.SalonSettingsManage)]
    public async Task<IActionResult> SetStatus(Guid id, [FromQuery] ClientFeedbackStatus status)
    {
        var f = await _db.ClientFeedbacks.FirstOrDefaultAsync(x => x.Id == id);
        if (f is null) return NotFound();
        f.Status = status;
        f.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Ok(f);
    }
}