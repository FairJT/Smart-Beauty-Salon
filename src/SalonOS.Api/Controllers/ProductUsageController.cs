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
        var u = new ProductUsage
        {
            BookingId = r.BookingId,
            ArtistId = artistId,
            InventoryItemId = r.InventoryItemId,
            Quantity = r.Quantity
        };
        _db.ProductUsages.Add(u);
        await _db.SaveChangesAsync();
        return Ok(u);
    }
}