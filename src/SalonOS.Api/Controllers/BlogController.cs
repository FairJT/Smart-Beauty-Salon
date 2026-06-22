using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using SalonOS.Shared.Authorization;
using SalonOS.Infrastructure;

namespace SalonOS.Api.Controllers;

[Route("api/blog")]
[ApiController]
public class BlogController : ControllerBase
{
    private readonly AppDbContext _db;
    public BlogController(AppDbContext db) => _db = db;

    public record PostReq(string Title, string Slug, string Body, PostType Type, string? Category, string? CoverImageUrl, bool IsPublished);

    // ── Public ──
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> List([FromQuery] PostType? type, [FromQuery] string? category)
    {
        var query = _db.BlogPosts.Where(p => p.IsPublished);
        if (type.HasValue) query = query.Where(p => p.Type == type.Value);
        if (!string.IsNullOrEmpty(category)) query = query.Where(p => p.Category == category);
        return Ok(await query.OrderByDescending(p => p.PublishedAt).ToListAsync());
    }

    [HttpGet("{slug}")]
    [AllowAnonymous]
    public async Task<IActionResult> BySlug(string slug)
    {
        var post = await _db.BlogPosts.FirstOrDefaultAsync(p => p.Slug == slug && p.IsPublished);
        return post is null ? NotFound() : Ok(post);
    }

    // ── Admin ──
    [HttpPost]
    [Authorize]
    [HasPermission(Permissions.PlatformConfigManage)]
    public async Task<IActionResult> Create([FromBody] PostReq r)
    {
        var post = new BlogPost
        {
            Title = r.Title,
            Slug = r.Slug,
            Body = r.Body,
            Type = r.Type,
            Category = r.Category,
            CoverImageUrl = r.CoverImageUrl,
            IsPublished = r.IsPublished,
            PublishedAt = r.IsPublished ? DateTime.UtcNow : null
        };
        _db.BlogPosts.Add(post);
        await _db.SaveChangesAsync();
        return Ok(post);
    }

    [HttpPut("{id}")]
    [Authorize]
    [HasPermission(Permissions.PlatformConfigManage)]
    public async Task<IActionResult> Update(Guid id, [FromBody] PostReq r)
    {
        var post = await _db.BlogPosts.FindAsync(id);
        if (post is null) return NotFound();

        post.Title = r.Title;
        post.Slug = r.Slug;
        post.Body = r.Body;
        post.Type = r.Type;
        post.Category = r.Category;
        post.CoverImageUrl = r.CoverImageUrl;

        if (r.IsPublished && !post.IsPublished) post.PublishedAt = DateTime.UtcNow;
        post.IsPublished = r.IsPublished;

        await _db.SaveChangesAsync();
        return Ok(post);
    }

    [HttpDelete("{id}")]
    [Authorize]
    [HasPermission(Permissions.PlatformConfigManage)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var post = await _db.BlogPosts.FindAsync(id);
        if (post is null) return NotFound();
        _db.BlogPosts.Remove(post);
        await _db.SaveChangesAsync();
        return Ok(new { deleted = true });
    }
}