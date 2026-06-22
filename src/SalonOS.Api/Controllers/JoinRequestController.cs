using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using SalonOS.Shared.Authorization;
using SalonOS.Infrastructure;

namespace SalonOS.Api.Controllers;

[Route("api/join-requests")]
[ApiController]
public class JoinRequestController : ControllerBase
{
    private readonly AppDbContext _db;
    public JoinRequestController(AppDbContext db) => _db = db;

    public record JoinReq(string SalonName, string OwnerName, string Phone, string? Email, string? City, string? Note);

    // Public: a prospective owner submits.
    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Submit([FromBody] JoinReq r)
    {
        var j = new SalonJoinRequest
        {
            SalonName = r.SalonName,
            OwnerName = r.OwnerName,
            Phone = r.Phone,
            Email = r.Email,
            City = r.City,
            Note = r.Note
        };
        _db.SalonJoinRequests.Add(j);
        await _db.SaveChangesAsync();
        return Ok(new { submitted = true, j.Id });
    }

    // Admin: review.
    [HttpGet]
    [Authorize]
    [HasPermission(Permissions.TenantManage)]
    public async Task<IActionResult> List([FromQuery] JoinRequestStatus? status)
    {
        var query = _db.SalonJoinRequests.AsQueryable();
        if (status.HasValue) query = query.Where(j => j.Status == status.Value);
        return Ok(await query.OrderByDescending(j => j.CreatedAt).ToListAsync());
    }

    [HttpPut("{id}/decision")]
    [Authorize]
    [HasPermission(Permissions.TenantManage)]
    public async Task<IActionResult> Decide(Guid id, [FromQuery] bool approve)
    {
        var j = await _db.SalonJoinRequests.FindAsync(id);
        if (j is null) return NotFound();
        j.Status = approve ? JoinRequestStatus.Approved : JoinRequestStatus.Rejected;
        await _db.SaveChangesAsync();
        return Ok(j);
    }
}