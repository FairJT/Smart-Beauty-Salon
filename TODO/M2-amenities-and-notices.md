# M2 — Salon amenities + notice board 🟡

Two simple tenant entities. Steps in order.

---

## Step 1 — entity: SalonAmenity
**New file:** `src/SalonOS.Infrastructure/SalonMgmt/SalonAmenity.cs`
```csharp
using SalonOS.Shared;

namespace SalonOS.Infrastructure;

public class SalonAmenity : TenantEntity
{
    public string Name { get; set; } = string.Empty;   // پارکینگ، فضای اسموک، کافی‌بار ...
    public string? Icon { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
```

## Step 2 — entity: SalonNotice
**New file:** `src/SalonOS.Infrastructure/SalonMgmt/SalonNotice.cs`
```csharp
using SalonOS.Shared;

namespace SalonOS.Infrastructure;

public class SalonNotice : TenantEntity
{
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public bool IsPinned { get; set; }
    public DateTime? StartsAt { get; set; }
    public DateTime? EndsAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
```

## Step 3 — register both DbSets
**File:** `src/SalonOS.Infrastructure/AppDbContext.cs`
**Find (exact):**
```csharp
    public DbSet<OutboxMessage> OutboxMessages { get; set; }
```
**Replace with:**
```csharp
    public DbSet<OutboxMessage> OutboxMessages { get; set; }
    public DbSet<SalonAmenity> SalonAmenities { get; set; }
    public DbSet<SalonNotice> SalonNotices { get; set; }
```

## Step 4 — migration
```powershell
dotnet ef migrations add SalonAmenitiesAndNotices --project src\SalonOS.Infrastructure --startup-project src\SalonOS.Api --context AppDbContext
```

## Step 5 — controller
**New file:** `src/SalonOS.Api/Controllers/SalonAmenityController.cs`
```csharp
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
        a.IsDeleted = true; a.DeletedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Ok(new { deleted = true });
    }
}
```

## Step 6 — controller
**New file:** `src/SalonOS.Api/Controllers/SalonNoticeController.cs`
```csharp
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
        Ok(await _db.SalonNotices.OrderByDescending(n => n.IsPinned).ThenByDescending(n => n.CreatedAt).ToListAsync());

    [HttpPost]
    [HasPermission(Permissions.SalonSettingsManage)]
    public async Task<IActionResult> Create([FromBody] NoticeRequest r)
    {
        var n = new SalonNotice { Title = r.Title, Body = r.Body, IsPinned = r.IsPinned, StartsAt = r.StartsAt, EndsAt = r.EndsAt };
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
        n.Title = r.Title; n.Body = r.Body; n.IsPinned = r.IsPinned; n.StartsAt = r.StartsAt; n.EndsAt = r.EndsAt; n.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Ok(n);
    }

    [HttpDelete("{id}")]
    [HasPermission(Permissions.SalonSettingsManage)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var n = await _db.SalonNotices.FirstOrDefaultAsync(x => x.Id == id);
        if (n is null) return NotFound();
        n.IsDeleted = true; n.DeletedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Ok(new { deleted = true });
    }
}
```

**Done when:** build succeeds; `GET /api/salon/amenities` and `GET /api/salon/notices` work for a manager.
