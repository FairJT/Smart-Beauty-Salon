# A5 — Record products consumed 🟡

Artist records which products were used in a visit. (Decrementing real inventory stock is a
follow-up Claude task — it crosses into the Inventory context.)

## Step 1 — entity
**New file:** `src/SalonOS.Infrastructure/SalonMgmt/ProductUsage.cs`
```csharp
using SalonOS.Shared;

namespace SalonOS.Infrastructure;

public class ProductUsage : TenantEntity
{
    public Guid BookingId { get; set; }
    public Guid ArtistId { get; set; }
    public Guid InventoryItemId { get; set; }
    public decimal Quantity { get; set; }
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
    public DbSet<ProductUsage> ProductUsages { get; set; }
```

## Step 3 — decimal precision config
**File:** `src/SalonOS.Infrastructure/AppDbContext.cs`
**Find (exact):**
```csharp
        base.OnModelCreating(builder);
```
**Replace with:**
```csharp
        base.OnModelCreating(builder);

        builder.Entity<ProductUsage>(e => e.Property(p => p.Quantity).HasColumnType("decimal(18,4)"));
```

## Step 4 — migration
```powershell
dotnet ef migrations add ProductUsage --project src\SalonOS.Infrastructure --startup-project src\SalonOS.Api --context AppDbContext
```

## Step 5 — controller
**New file:** `src/SalonOS.Api/Controllers/ProductUsageController.cs`
```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using SalonOS.Shared.Authorization;
using SalonOS.Infrastructure;

namespace SalonOS.Api.Controllers;

[Route("api/product-usage")]
[ApiController]
[Authorize]
public class ProductUsageController : ControllerBase
{
    private readonly AppDbContext _db;
    public ProductUsageController(AppDbContext db) => _db = db;

    public record UsageRequest(Guid BookingId, Guid InventoryItemId, decimal Quantity);

    private bool TryArtist(out Guid artistId)
    {
        artistId = Guid.Empty;
        var v = User.FindFirst("artist_id")?.Value;
        return !string.IsNullOrEmpty(v) && Guid.TryParse(v, out artistId);
    }

    [HttpGet("by-booking/{bookingId}")]
    [HasPermission(Permissions.ProductUsageRecord)]
    public async Task<IActionResult> ByBooking(Guid bookingId) =>
        Ok(await _db.ProductUsages.Where(u => u.BookingId == bookingId).ToListAsync());

    [HttpPost]
    [HasPermission(Permissions.ProductUsageRecord)]
    public async Task<IActionResult> Record([FromBody] UsageRequest r)
    {
        if (!TryArtist(out var artistId)) return Forbid();
        var u = new ProductUsage { BookingId = r.BookingId, ArtistId = artistId, InventoryItemId = r.InventoryItemId, Quantity = r.Quantity };
        _db.ProductUsages.Add(u);
        await _db.SaveChangesAsync();
        return Ok(u);
        // NOTE: decrementing the InventoryItem's OnHandQty is a follow-up (Claude) — cross-context.
    }
}
```

**Done when:** build succeeds; an artist can record and list product usage per booking.
