# M6 — Discounts / coupon codes 🟡

General or coded discounts for occasions; a code can target a specific client. Percent or fixed
amount (amount stored as plain Rial minor units, not an owned Money — keeps it simple/nullable).

## Step 1 — entity
**New file:** `src/SalonOS.Infrastructure/SalonMgmt/Discount.cs`
```csharp
using SalonOS.Shared;

namespace SalonOS.Infrastructure;

public class Discount : TenantEntity
{
    public string? Code { get; set; }            // null = general (auto), set = coupon code
    public int? Percent { get; set; }            // either Percent ...
    public long? AmountMinor { get; set; }       // ... or a fixed amount (Rial minor units)
    public DateTime StartsAt { get; set; }
    public DateTime EndsAt { get; set; }
    public string? TargetClientId { get; set; }  // null = anyone; set = one client
    public int? MaxUses { get; set; }
    public int UsedCount { get; set; }
    public bool IsActive { get; set; } = true;
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
    public DbSet<Discount> Discounts { get; set; }
```

## Step 3 — migration
```powershell
dotnet ef migrations add SalonDiscounts --project src\SalonOS.Infrastructure --startup-project src\SalonOS.Api --context AppDbContext
```

## Step 4 — controller
**New file:** `src/SalonOS.Api/Controllers/DiscountController.cs`
```csharp
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
        Ok(await _db.Discounts.Where(d => d.IsActive).OrderByDescending(d => d.StartsAt).ToListAsync());

    [HttpPost]
    [HasPermission(Permissions.SalonSettingsManage)]
    public async Task<IActionResult> Create([FromBody] DiscountRequest r)
    {
        if (r.Percent is null && r.AmountMinor is null)
            return BadRequest(new { message = "Provide either Percent or AmountMinor" });
        var d = new Discount
        {
            Code = r.Code, Percent = r.Percent, AmountMinor = r.AmountMinor,
            StartsAt = r.StartsAt, EndsAt = r.EndsAt, TargetClientId = r.TargetClientId, MaxUses = r.MaxUses
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
        d.IsActive = false; d.IsDeleted = true; d.DeletedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Ok(new { deleted = true });
    }
}
```

**Done when:** build succeeds; manager can create/list/delete discounts.
**⚠️ Review:** applying a discount at booking time (validate code, check MaxUses/dates/target, increment UsedCount) is a later Claude task — this only manages them.
