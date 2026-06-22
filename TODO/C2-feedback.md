# C2 — Suggestion / complaint (item 16) 🟡

Mirror of the artist's `StaffRequest`, but client → salon. The client must be in a salon's context
(same as booking) so the target tenant is stamped automatically.

## Step 1 — entity
**New file:** `src/SalonOS.Infrastructure/SalonMgmt/ClientFeedback.cs`
```csharp
using SalonOS.Shared;

namespace SalonOS.Infrastructure;

public enum ClientFeedbackType { Suggestion = 1, Complaint = 2 }
public enum ClientFeedbackStatus { Open = 1, InProgress = 2, Resolved = 3 }

public class ClientFeedback : TenantEntity
{
    public string ClientId { get; set; } = string.Empty;   // the user's id
    public ClientFeedbackType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Detail { get; set; }
    public ClientFeedbackStatus Status { get; set; } = ClientFeedbackStatus.Open;
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
    public DbSet<ClientFeedback> ClientFeedbacks { get; set; }
```

## Step 3 — migration
```powershell
dotnet ef migrations add ClientFeedback --project src\SalonOS.Infrastructure --startup-project src\SalonOS.Api --context AppDbContext
```

## Step 4 — controller (client create + own list; manager view/resolve)
**New file:** `src/SalonOS.Api/Controllers/ClientFeedbackController.cs`
```csharp
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
        f.Status = status; f.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Ok(f);
    }
}
```

**Done when:** build succeeds; client can submit/list-own feedback, manager can list-all/resolve.
