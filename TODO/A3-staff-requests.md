# A3 — Report issues + equipment requests 🟡

One entity for items 16 + 17. Artist creates; manager sees and resolves.

## Step 1 — entity
**New file:** `src/SalonOS.Infrastructure/SalonMgmt/StaffRequest.cs`
```csharp
using SalonOS.Shared;

namespace SalonOS.Infrastructure;

public enum StaffRequestType { Issue = 1, Equipment = 2 }
public enum StaffRequestStatus { Open = 1, InProgress = 2, Resolved = 3 }

public class StaffRequest : TenantEntity
{
    public Guid ArtistId { get; set; }
    public StaffRequestType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Detail { get; set; }
    public StaffRequestStatus Status { get; set; } = StaffRequestStatus.Open;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
```

## Step 2 — register DbSet
**File:** `src/SalonOS.Infrastructure/AppDbContext.cs`
**Find (exact):**
```csharp
    public DbSet<OutboxMessage> OutboxMessages { get; set; }
```
**Replace with:**
```csharp
    public DbSet<OutboxMessage> OutboxMessages { get; set; }
    public DbSet<StaffRequest> StaffRequests { get; set; }
```

## Step 3 — migration
```powershell
dotnet ef migrations add StaffRequests --project src\SalonOS.Infrastructure --startup-project src\SalonOS.Api --context AppDbContext
```

## Step 4 — controller (artist create/list-own + manager list-all/resolve)
**New file:** `src/SalonOS.Api/Controllers/StaffRequestController.cs`
```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using SalonOS.Shared.Authorization;
using SalonOS.Infrastructure;

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

    // Artist: create + see own
    [HttpPost]
    [HasPermission(Permissions.StaffRequestCreate)]
    public async Task<IActionResult> Create([FromBody] CreateReq r)
    {
        if (!TryArtist(out var artistId)) return Forbid();
        var sr = new StaffRequest { ArtistId = artistId, Type = r.Type, Title = r.Title, Detail = r.Detail };
        _db.StaffRequests.Add(sr);
        await _db.SaveChangesAsync();
        return Ok(sr);
    }

    [HttpGet("mine")]
    [HasPermission(Permissions.StaffRequestCreate)]
    public async Task<IActionResult> Mine()
    {
        if (!TryArtist(out var artistId)) return Forbid();
        return Ok(await _db.StaffRequests.Where(s => s.ArtistId == artistId).OrderByDescending(s => s.CreatedAt).ToListAsync());
    }

    // Manager: see all + resolve
    [HttpGet]
    [HasPermission(Permissions.SalonView)]
    public async Task<IActionResult> All([FromQuery] StaffRequestStatus? status)
    {
        var q = _db.StaffRequests.AsQueryable();
        if (status.HasValue) q = q.Where(s => s.Status == status.Value);
        return Ok(await q.OrderByDescending(s => s.CreatedAt).ToListAsync());
    }

    [HttpPut("{id}/status")]
    [HasPermission(Permissions.SalonSettingsManage)]
    public async Task<IActionResult> SetStatus(Guid id, [FromQuery] StaffRequestStatus status)
    {
        var sr = await _db.StaffRequests.FirstOrDefaultAsync(x => x.Id == id);
        if (sr is null) return NotFound();
        sr.Status = status; sr.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Ok(sr);
    }
}
```

**Done when:** build succeeds; artist can file/list-own requests, manager can list-all/resolve.
