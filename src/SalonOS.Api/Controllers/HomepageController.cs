using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using SalonOS.Shared.Authorization;
using SalonOS.Infrastructure;

namespace SalonOS.Api.Controllers;

[Route("api/homepage")]
[ApiController]
public class HomepageController : ControllerBase
{
    private readonly AppDbContext _db;
    public HomepageController(AppDbContext db) => _db = db;

    public record SlideReq(string Title, string ImageUrl, string? LinkUrl, int SortOrder, bool IsActive);
    public record MenuReq(MenuLocation Location, string Title, string Url, int SortOrder, bool IsActive);

    // ── Public read (homepage) ──
    [HttpGet("slides")]
    [AllowAnonymous]
    public async Task<IActionResult> Slides() =>
        Ok(await _db.HomepageSlides.Where(s => s.IsActive).OrderBy(s => s.SortOrder).ToListAsync());

    [HttpGet("menus")]
    [AllowAnonymous]
    public async Task<IActionResult> Menus([FromQuery] MenuLocation? location)
    {
        var query = _db.HomepageMenus.Where(m => m.IsActive);
        if (location.HasValue) query = query.Where(m => m.Location == location.Value);
        return Ok(await query.OrderBy(m => m.SortOrder).ToListAsync());
    }

    // ── Admin manage ──
    [HttpPost("slides")]
    [Authorize]
    [HasPermission(Permissions.PlatformConfigManage)]
    public async Task<IActionResult> AddSlide([FromBody] SlideReq r)
    {
        var slide = new HomepageSlide
        {
            Title = r.Title,
            ImageUrl = r.ImageUrl,
            LinkUrl = r.LinkUrl,
            SortOrder = r.SortOrder,
            IsActive = r.IsActive
        };
        _db.HomepageSlides.Add(slide);
        await _db.SaveChangesAsync();
        return Ok(slide);
    }

    [HttpDelete("slides/{id}")]
    [Authorize]
    [HasPermission(Permissions.PlatformConfigManage)]
    public async Task<IActionResult> DeleteSlide(Guid id)
    {
        var slide = await _db.HomepageSlides.FindAsync(id);
        if (slide is null) return NotFound();
        _db.HomepageSlides.Remove(slide);
        await _db.SaveChangesAsync();
        return Ok(new { deleted = true });
    }

    [HttpPost("menus")]
    [Authorize]
    [HasPermission(Permissions.PlatformConfigManage)]
    public async Task<IActionResult> AddMenu([FromBody] MenuReq r)
    {
        var menu = new HomepageMenu
        {
            Location = r.Location,
            Title = r.Title,
            Url = r.Url,
            SortOrder = r.SortOrder,
            IsActive = r.IsActive
        };
        _db.HomepageMenus.Add(menu);
        await _db.SaveChangesAsync();
        return Ok(menu);
    }

    [HttpDelete("menus/{id}")]
    [Authorize]
    [HasPermission(Permissions.PlatformConfigManage)]
    public async Task<IActionResult> DeleteMenu(Guid id)
    {
        var menu = await _db.HomepageMenus.FindAsync(id);
        if (menu is null) return NotFound();
        _db.HomepageMenus.Remove(menu);
        await _db.SaveChangesAsync();
        return Ok(new { deleted = true });
    }
}