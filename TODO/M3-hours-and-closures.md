# M3 — Working hours + salon closures 🟡

Times stored as `"HH:mm"` strings (simple + safe). Closures override the weekly hours for a date.

## Step 1 — entity: WorkingHour
**New file:** `src/SalonOS.Infrastructure/SalonMgmt/WorkingHour.cs`
```csharp
using SalonOS.Shared;

namespace SalonOS.Infrastructure;

public class WorkingHour : TenantEntity
{
    public int DayOfWeek { get; set; }              // 0=Sat ... 6=Fri (decide one convention and keep it)
    public string OpenTime { get; set; } = "09:00"; // "HH:mm"
    public string CloseTime { get; set; } = "21:00";
    public bool IsClosed { get; set; }              // weekly day off
}
```

## Step 2 — entity: SalonClosure
**New file:** `src/SalonOS.Infrastructure/SalonMgmt/SalonClosure.cs`
```csharp
using SalonOS.Shared;

namespace SalonOS.Infrastructure;

// Overrides the weekly hours for a specific date.
// IsClosed=true → closed that day; IsClosed=false → OPEN even if it's an official holiday/Friday.
public class SalonClosure : TenantEntity
{
    public DateTime Date { get; set; }
    public bool IsClosed { get; set; } = true;
    public string? Reason { get; set; }
}
```

## Step 3 — register DbSets
**File:** `src/SalonOS.Infrastructure/AppDbContext.cs`
**Find (exact):**
```csharp
    public DbSet<OutboxMessage> OutboxMessages { get; set; }
```
**Replace with:**
```csharp
    public DbSet<OutboxMessage> OutboxMessages { get; set; }
    public DbSet<WorkingHour> WorkingHours { get; set; }
    public DbSet<SalonClosure> SalonClosures { get; set; }
```

## Step 4 — migration
```powershell
dotnet ef migrations add SalonHoursAndClosures --project src\SalonOS.Infrastructure --startup-project src\SalonOS.Api --context AppDbContext
```

## Step 5 — controller (both, in one file)
**New file:** `src/SalonOS.Api/Controllers/WorkingHoursController.cs`
```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using SalonOS.Shared.Authorization;
using SalonOS.Infrastructure;

namespace SalonOS.Api.Controllers;

[Route("api/salon")]
[ApiController]
[Authorize]
public class WorkingHoursController : ControllerBase
{
    private readonly AppDbContext _db;
    public WorkingHoursController(AppDbContext db) => _db = db;

    public record HourRequest(int DayOfWeek, string OpenTime, string CloseTime, bool IsClosed);
    public record ClosureRequest(DateTime Date, bool IsClosed, string? Reason);

    // ── Weekly hours ──────────────────────────────
    [HttpGet("working-hours")]
    [HasPermission(Permissions.SalonView)]
    public async Task<IActionResult> GetHours() =>
        Ok(await _db.WorkingHours.OrderBy(h => h.DayOfWeek).ToListAsync());

    [HttpPut("working-hours")]
    [HasPermission(Permissions.SalonSettingsManage)]
    public async Task<IActionResult> SetHours([FromBody] List<HourRequest> rows)
    {
        var existing = await _db.WorkingHours.ToListAsync();
        _db.WorkingHours.RemoveRange(existing);                  // replace the whole week
        foreach (var r in rows)
            _db.WorkingHours.Add(new WorkingHour { DayOfWeek = r.DayOfWeek, OpenTime = r.OpenTime, CloseTime = r.CloseTime, IsClosed = r.IsClosed });
        await _db.SaveChangesAsync();
        return Ok(new { saved = rows.Count });
    }

    // ── Date closures ─────────────────────────────
    [HttpGet("closures")]
    [HasPermission(Permissions.SalonView)]
    public async Task<IActionResult> GetClosures() =>
        Ok(await _db.SalonClosures.OrderBy(c => c.Date).ToListAsync());

    [HttpPost("closures")]
    [HasPermission(Permissions.SalonSettingsManage)]
    public async Task<IActionResult> AddClosure([FromBody] ClosureRequest r)
    {
        var c = new SalonClosure { Date = r.Date.Date, IsClosed = r.IsClosed, Reason = r.Reason };
        _db.SalonClosures.Add(c);
        await _db.SaveChangesAsync();
        return Ok(c);
    }

    [HttpDelete("closures/{id}")]
    [HasPermission(Permissions.SalonSettingsManage)]
    public async Task<IActionResult> DeleteClosure(Guid id)
    {
        var c = await _db.SalonClosures.FirstOrDefaultAsync(x => x.Id == id);
        if (c is null) return NotFound();
        c.IsDeleted = true; c.DeletedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Ok(new { deleted = true });
    }
}
```

**Done when:** build succeeds; manager can set the weekly hours and add/remove date closures.

**⚠️ Review (later, not this task):** the booking availability engine should consult these (a day is
open if its `WorkingHour` says so, UNLESS a `SalonClosure` for that date overrides it). That wiring is a Claude task.
