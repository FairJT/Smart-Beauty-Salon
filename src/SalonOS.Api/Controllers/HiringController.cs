using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using SalonOS.Shared.Authorization;
using SalonOS.Infrastructure;

namespace SalonOS.Api.Controllers;

[Route("api/salon/hiring")]
[ApiController]
[Authorize]
public class HiringController : ControllerBase
{
    private readonly AppDbContext _db;
    public HiringController(AppDbContext db) => _db = db;

    public record PostingRequest(string Title, HireKind Kind, string? Description, string? Location, bool IsUrgent);
    public record ApplicationDecisionRequest(bool Approve);

    // ── Postings ──────────────────────────────────
    [HttpGet("postings")]
    [HasPermission(Permissions.JobPostingView)]
    public async Task<IActionResult> ListPostings() =>
        Ok(await _db.JobPostings.Where(p => p.IsActive).OrderByDescending(p => p.CreatedAt).ToListAsync());

    [HttpPost("postings")]
    [HasPermission(Permissions.JobPostingManage)]
    public async Task<IActionResult> CreatePosting([FromBody] PostingRequest r)
    {
        var p = new JobPosting
        {
            Title = r.Title,
            Kind = r.Kind,
            Description = r.Description,
            Location = r.Location,
            IsUrgent = r.IsUrgent
        };
        _db.JobPostings.Add(p);
        await _db.SaveChangesAsync();
        return Ok(p);
    }

    [HttpDelete("postings/{id}")]
    [HasPermission(Permissions.JobPostingManage)]
    public async Task<IActionResult> ClosePosting(Guid id)
    {
        var p = await _db.JobPostings.FirstOrDefaultAsync(x => x.Id == id);
        if (p is null) return NotFound();
        p.IsActive = false;
        p.IsDeleted = true;
        p.DeletedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Ok(new { closed = true });
    }

    // ── Applications ──────────────────────────────
    [HttpGet("applications")]
    [HasPermission(Permissions.JobPostingManage)]
    public async Task<IActionResult> ListApplications([FromQuery] Guid? postingId)
    {
        var query = _db.JobApplications.AsQueryable();
        if (postingId.HasValue) query = query.Where(a => a.JobPostingId == postingId.Value);
        return Ok(await query.OrderByDescending(a => a.CreatedAt).ToListAsync());
    }

    [HttpPut("applications/{id}/decision")]
    [HasPermission(Permissions.JobPostingManage)]
    public async Task<IActionResult> Decide(Guid id, [FromQuery] bool approve)
    {
        var a = await _db.JobApplications.FirstOrDefaultAsync(x => x.Id == id);
        if (a is null) return NotFound();
        a.Status = approve ? ApplicationStatus.Approved : ApplicationStatus.Rejected;
        a.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Ok(a);
    }
}