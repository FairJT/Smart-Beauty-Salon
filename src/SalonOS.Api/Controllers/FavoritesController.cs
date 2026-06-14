using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SalonOS.Identity.Domain;
using SalonOS.Identity.Infrastructure;
using SalonOS.Shared.Authorization;
using System.Security.Claims;

namespace SalonOS.Api.Controllers;

[Route("api/me/favorites")]
[ApiController]
public class FavoritesController : ControllerBase
{
    private readonly IdentityDbContext _db;

    public FavoritesController(IdentityDbContext db) => _db = db;

    [HttpGet]
    [HasPermission(Permissions.AppointmentCreate)]
    public async Task<IActionResult> GetFavorites()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var favorites = await _db.SavedSalons
            .Where(s => s.UserId == userId)
            .Select(s => new
            {
                salonId = s.SalonId,
                salonName = s.SalonName,
                logoUrl = s.LogoUrl,
                createdAt = s.CreatedAt
            })
            .OrderByDescending(s => s.createdAt)
            .ToListAsync();

        return Ok(favorites);
    }

    [HttpPost("{salonId}")]
    [HasPermission(Permissions.AppointmentCreate)]
    public async Task<IActionResult> AddFavorite(int salonId, [FromBody] AddFavoriteRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var alreadySaved = await _db.SavedSalons
            .AnyAsync(s => s.UserId == userId && s.SalonId == salonId);
        if (alreadySaved)
            return Conflict(new { message = "Salon already in favorites" });

        var saved = new SavedSalon
        {
            UserId = userId,
            SalonId = salonId,
            SalonName = request.SalonName ?? "",
            LogoUrl = request.LogoUrl
        };

        _db.SavedSalons.Add(saved);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetFavorites), new { id = saved.Id }, saved);
    }

    [HttpDelete("{salonId}")]
    [HasPermission(Permissions.AppointmentCreate)]
    public async Task<IActionResult> RemoveFavorite(int salonId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var saved = await _db.SavedSalons
            .FirstOrDefaultAsync(s => s.UserId == userId && s.SalonId == salonId);
        if (saved == null)
            return NotFound(new { message = "Favorite not found" });

        _db.SavedSalons.Remove(saved);
        await _db.SaveChangesAsync();

        return NoContent();
    }
}

public class AddFavoriteRequest
{
    public string? SalonName { get; set; }
    public string? LogoUrl { get; set; }
}
