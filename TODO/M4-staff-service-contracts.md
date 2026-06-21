# M4 — Per-service staff contracts + discount 🟡

Each artist can rent / be contracted per service (line), each with its own terms. This is the
"contract per service" the spec wants (the single `ArtistProfile.ContractType` is too coarse).

## Step 1 — entity
**New file:** `src/SalonOS.Infrastructure/SalonMgmt/StaffServiceContract.cs`
```csharp
using SalonOS.Shared;

namespace SalonOS.Infrastructure;

public enum StaffContractKind { Percentage = 1, Rental = 2, FixedSalary = 3 }

public class StaffServiceContract : TenantEntity
{
    public Guid ArtistId { get; set; }
    public Guid CatalogServiceId { get; set; }     // the line (parent service)
    public StaffContractKind Kind { get; set; } = StaffContractKind.Rental;
    public Money Amount { get; set; } = Money.Zero("IRR");   // salary / rent / percentage base
    public int? DiscountPercent { get; set; }      // non-fixed staff may discount their service
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? ContractFileUrl { get; set; }
    public string? GuaranteeNote { get; set; }     // سفته/چک ضمانت
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
    public DbSet<StaffServiceContract> StaffServiceContracts { get; set; }
```

## Step 3 — configure the Money field
**File:** `src/SalonOS.Infrastructure/AppDbContext.cs`
**Find (exact):**
```csharp
        base.OnModelCreating(builder);
```
**Replace with:**
```csharp
        base.OnModelCreating(builder);

        builder.Entity<StaffServiceContract>(e => e.OwnsOne(c => c.Amount));
```

## Step 4 — migration
```powershell
dotnet ef migrations add StaffServiceContracts --project src\SalonOS.Infrastructure --startup-project src\SalonOS.Api --context AppDbContext
```

## Step 5 — controller
**New file:** `src/SalonOS.Api/Controllers/StaffServiceContractController.cs`
```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using SalonOS.Shared;
using SalonOS.Shared.Authorization;
using SalonOS.Infrastructure;

namespace SalonOS.Api.Controllers;

[Route("api/staff-contracts")]
[ApiController]
[Authorize]
public class StaffServiceContractController : ControllerBase
{
    private readonly AppDbContext _db;
    public StaffServiceContractController(AppDbContext db) => _db = db;

    public record ContractRequest(
        Guid ArtistId, Guid CatalogServiceId, StaffContractKind Kind,
        long AmountValue, string Currency, int? DiscountPercent,
        DateTime StartDate, DateTime? EndDate, string? ContractFileUrl, string? GuaranteeNote);

    [HttpGet]
    [HasPermission(Permissions.StaffView)]
    public async Task<IActionResult> List([FromQuery] Guid? artistId)
    {
        var q = _db.StaffServiceContracts.Where(c => c.IsActive);
        if (artistId.HasValue) q = q.Where(c => c.ArtistId == artistId.Value);
        return Ok(await q.ToListAsync());
    }

    [HttpPost]
    [HasPermission(Permissions.StaffContractManage)]
    public async Task<IActionResult> Create([FromBody] ContractRequest r)
    {
        var c = new StaffServiceContract
        {
            ArtistId = r.ArtistId, CatalogServiceId = r.CatalogServiceId, Kind = r.Kind,
            Amount = Money.Of(r.AmountValue, r.Currency), DiscountPercent = r.DiscountPercent,
            StartDate = r.StartDate, EndDate = r.EndDate, ContractFileUrl = r.ContractFileUrl, GuaranteeNote = r.GuaranteeNote
        };
        _db.StaffServiceContracts.Add(c);
        await _db.SaveChangesAsync();
        return Ok(c);
    }

    [HttpDelete("{id}")]
    [HasPermission(Permissions.StaffContractManage)]
    public async Task<IActionResult> End(Guid id)
    {
        var c = await _db.StaffServiceContracts.FirstOrDefaultAsync(x => x.Id == id);
        if (c is null) return NotFound();
        c.IsActive = false; c.IsDeleted = true; c.DeletedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Ok(new { ended = true });
    }
}
```

**Done when:** build succeeds; manager can list/create/end per-service contracts.
**⚠️ Review:** `DiscountPercent` should only be allowed when `Kind != FixedSalary` — enforce in a later validation task.
