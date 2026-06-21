# M7 — Hiring: job postings + applications 🟡

The salon posts jobs and reviews applications. (The cross-tenant browse side — where jobseekers
search all salons — is a separate Claude task; here postings are tenant-owned.)

## Step 1 — entities
**New file:** `src/SalonOS.Infrastructure/SalonMgmt/JobPosting.cs`
```csharp
using SalonOS.Shared;

namespace SalonOS.Infrastructure;

public enum HireKind { Internship = 1, LineRental = 2, FixedSalary = 3, Percentage = 4 }
public enum ApplicationStatus { Pending = 1, Approved = 2, Rejected = 3 }

public class JobPosting : TenantEntity
{
    public string Title { get; set; } = string.Empty;
    public HireKind Kind { get; set; }
    public string? Description { get; set; }
    public string? Location { get; set; }
    public bool IsUrgent { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class JobApplication : TenantEntity
{
    public Guid JobPostingId { get; set; }
    public string ApplicantUserId { get; set; } = string.Empty;
    public ApplicationStatus Status { get; set; } = ApplicationStatus.Pending;
    public string? Message { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
```

## Step 2 — register DbSets
**File:** `src/SalonOS.Infrastructure/AppDbContext.cs`
**Find (exact):**
```csharp
    public DbSet<OutboxMessage> OutboxMessages { get; set; }
```
**Replace with:**
```csharp
    public DbSet<OutboxMessage> OutboxMessages { get; set; }
    public DbSet<JobPosting> JobPostings { get; set; }
    public DbSet<JobApplication> JobApplications { get; set; }
```

## Step 3 — migration
```powershell
dotnet ef migrations add Hiring --project src\SalonOS.Infrastructure --startup-project src\SalonOS.Api --context AppDbContext
```

## Step 4 — controller
**New file:** `src/SalonOS.Api/Controllers/HiringController.cs`
```csharp
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

    // ── Postings ──────────────────────────────────
    [HttpGet("postings")]
    [HasPermission(Permissions.JobPostingView)]
    public async Task<IActionResult> ListPostings() =>
        Ok(await _db.JobPostings.Where(p => p.IsActive).OrderByDescending(p => p.CreatedAt).ToListAsync());

    [HttpPost("postings")]
    [HasPermission(Permissions.JobPostingManage)]
    public async Task<IActionResult> CreatePosting([FromBody] PostingRequest r)
    {
        var p = new JobPosting { Title = r.Title, Kind = r.Kind, Description = r.Description, Location = r.Location, IsUrgent = r.IsUrgent };
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
        p.IsActive = false; p.IsDeleted = true; p.DeletedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Ok(new { closed = true });
    }

    // ── Applications ──────────────────────────────
    [HttpGet("applications")]
    [HasPermission(Permissions.JobPostingManage)]
    public async Task<IActionResult> ListApplications([FromQuery] Guid? postingId)
    {
        var q = _db.JobApplications.AsQueryable();
        if (postingId.HasValue) q = q.Where(a => a.JobPostingId == postingId.Value);
        return Ok(await q.OrderByDescending(a => a.CreatedAt).ToListAsync());
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
```

## Step 5 — give the manager the hiring permissions
**File:** `src/SalonOS.Shared/Authorization/RolePermissions.cs`
**Find (exact):**
```csharp
                Permissions.MarketplaceBrowse,
                Permissions.MarketplaceLicensePurchase,
            },
```
**Replace with:**
```csharp
                Permissions.MarketplaceBrowse,
                Permissions.MarketplaceLicensePurchase,
                Permissions.JobPostingView,
                Permissions.JobPostingManage,
            },
```
(This is the FIRST `},` — it closes the `["SalonManager"]` block. If unsure, confirm the two lines
above the match are the Marketplace ones inside SalonManager.)

**Done when:** build succeeds; manager can post jobs, list applications, and approve/reject.
