# C3 — View discounts + validate a code (items 10, 17-safe) 🟡

Read-only. The client sees active discounts and can check if a code is valid. Applying the code to a
price and incrementing `UsedCount` is money-critical → that's a Claude task, NOT here.

No new entity — uses the `Discount` table from the manager batch (M6).

## Step 1 — controller
**New file:** `src/SalonOS.Api/Controllers/ClientOffersController.cs`
```csharp
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using SalonOS.Shared.Authorization;
using SalonOS.Infrastructure;

namespace SalonOS.Api.Controllers;

[Route("api/offers")]
[ApiController]
[Authorize]
public class ClientOffersController : ControllerBase
{
    private readonly AppDbContext _db;
    public ClientOffersController(AppDbContext db) => _db = db;

    private string? Me() => User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    // Active discounts for this salon that apply to me (general or targeted at me).
    [HttpGet("discounts")]
    [HasPermission(Permissions.ClientSelf)]
    public async Task<IActionResult> ActiveDiscounts()
    {
        var me = Me();
        var now = DateTime.UtcNow;
        var list = await _db.Discounts
            .Where(d => d.IsActive && d.StartsAt <= now && d.EndsAt >= now
                && (d.TargetClientId == null || d.TargetClientId == me))
            .OrderByDescending(d => d.StartsAt)
            .ToListAsync();
        return Ok(list);
    }

    // Validate a coupon code (read-only — does NOT consume it).
    [HttpGet("discounts/validate")]
    [HasPermission(Permissions.ClientSelf)]
    public async Task<IActionResult> Validate([FromQuery] string code)
    {
        if (string.IsNullOrWhiteSpace(code)) return BadRequest(new { message = "code required" });
        var me = Me();
        var now = DateTime.UtcNow;
        var d = await _db.Discounts.FirstOrDefaultAsync(x =>
            x.Code == code && x.IsActive && x.StartsAt <= now && x.EndsAt >= now
            && (x.TargetClientId == null || x.TargetClientId == me));

        if (d is null)
            return Ok(new { valid = false });
        if (d.MaxUses.HasValue && d.UsedCount >= d.MaxUses.Value)
            return Ok(new { valid = false, reason = "max-uses-reached" });

        return Ok(new { valid = true, d.Id, d.Percent, d.AmountMinor });
    }
}
```

**Done when:** build succeeds; `GET /api/offers/discounts` and `GET /api/offers/discounts/validate?code=...` work for a client.

**⚠️ Note:** actually applying the discount at checkout (and incrementing `UsedCount`) is reserved for
Claude because it touches money + the booking/payment flow.
