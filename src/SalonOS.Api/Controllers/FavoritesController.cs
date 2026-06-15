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
    [HasPermission(Permissions.ClientSelf)]
    public async Task<IActionResult> GetFavorites()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var saved = await _db.SavedSalons
            .Where(s => s.UserId == userId)
            .ToListAsync();

        var slugs = saved.Select(s => s.Slug).ToList();
        var currentTenants = await _db.Tenants
            .Where(t => slugs.Contains(t.Slug))
            .Select(t => new { t.Slug, t.Name, t.LogoUrl })
            .ToListAsync();

        var tenantMap = currentTenants.ToDictionary(t => t.Slug);
        var staleCount = 0;

        foreach (var s in saved)
        {
            if (tenantMap.TryGetValue(s.Slug, out var tenant))
            {
                if (s.SalonName != tenant.Name || s.LogoUrl != tenant.LogoUrl)
                {
                    s.SalonName = tenant.Name;
                    s.LogoUrl = tenant.LogoUrl;
                    staleCount++;
                }
            }
        }

        if (staleCount > 0)
            await _db.SaveChangesAsync();

        var favorites = saved
            .OrderByDescending(s => s.CreatedAt)
            .Select(s =>
            {
                var tenant = tenantMap.GetValueOrDefault(s.Slug);
                return new
                {
                    slug = s.Slug,
                    salonName = s.SalonName,
                    logoUrl = s.LogoUrl,
                    createdAt = s.CreatedAt,
                    ratingAvg = 0.0,
                    isVip = false
                };
            })
            .ToList();

        return Ok(favorites);
    }

    [HttpPost("{slug}")]
    [HasPermission(Permissions.ClientSelf)]
    public async Task<IActionResult> AddFavorite(string slug, [FromBody] AddFavoriteRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var alreadySaved = await _db.SavedSalons
            .AnyAsync(s => s.UserId == userId && s.Slug == slug);
        if (alreadySaved)
            return Conflict(new { message = "Salon already in favorites" });

        var saved = new SavedSalon
        {
            UserId = userId,
            Slug = slug,
            SalonName = request.SalonName ?? "",
            LogoUrl = request.LogoUrl
        };

        _db.SavedSalons.Add(saved);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetFavorites), new { id = saved.Id }, saved);
    }

    [HttpPut("{slug}/refresh")]
    [HasPermission(Permissions.ClientSelf)]
    public async Task<IActionResult> RefreshFavorite(string slug)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var saved = await _db.SavedSalons
            .FirstOrDefaultAsync(s => s.UserId == userId && s.Slug == slug);
        if (saved == null)
            return NotFound(new { message = "Favorite not found" });

        var tenant = await _db.Tenants
            .Where(t => t.Slug == slug)
            .Select(t => new { t.Name, t.LogoUrl })
            .FirstOrDefaultAsync();

        if (tenant != null)
        {
            saved.SalonName = tenant.Name;
            saved.LogoUrl = tenant.LogoUrl;
            await _db.SaveChangesAsync();
        }

        return Ok(new
        {
            slug = saved.Slug,
            salonName = saved.SalonName,
            logoUrl = saved.LogoUrl
        });
    }

    [HttpDelete("{slug}")]
    [HasPermission(Permissions.ClientSelf)]
    public async Task<IActionResult> RemoveFavorite(string slug)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var saved = await _db.SavedSalons
            .FirstOrDefaultAsync(s => s.UserId == userId && s.Slug == slug);
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
