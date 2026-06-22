using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using SalonOS.Shared.Authorization;
using SalonOS.Shared;
using SalonOS.Shared.Identity;
using SalonOS.Infrastructure;

namespace SalonOS.Api.Controllers;

[Route("api/client-notes")]
[ApiController]
[Authorize]
public class ClientNoteController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ICurrentUser _currentUser;

    public ClientNoteController(AppDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public record NoteRequest(string ClientId, ClientNoteType Type, string Text);

    private bool TryArtist(out Guid artistId)
    {
        artistId = Guid.Empty;
        var v = _currentUser.UserId;
        return !string.IsNullOrEmpty(v) && Guid.TryParse(v, out artistId);
    }

    // Notes this artist wrote, optionally for one client.
    [HttpGet]
    [HasPermission(Permissions.ClientNoteManageOwn)]
    public async Task<IActionResult> List([FromQuery] string? clientId)
    {
        if (!TryArtist(out var artistId)) return Forbid();
        var q = _db.ClientNotes.Where(n => n.ArtistId == artistId);
        if (!string.IsNullOrEmpty(clientId)) q = q.Where(n => n.ClientId == clientId);
        return Ok(await q.OrderByDescending(n => n.CreatedAt).ToListAsync());
    }

    [HttpPost]
    [HasPermission(Permissions.ClientNoteManageOwn)]
    public async Task<IActionResult> Create([FromBody] NoteRequest r)
    {
        if (!TryArtist(out var artistId)) return Forbid();
        var n = new ClientNote
        {
            ArtistId = artistId,
            ClientId = r.ClientId,
            Type = r.Type,
            Text = r.Text
        };
        _db.ClientNotes.Add(n);
        await _db.SaveChangesAsync();
        return Ok(n);
    }

    [HttpDelete("{id:guid}")]
    [HasPermission(Permissions.ClientNoteManageOwn)]
    public async Task<IActionResult> Delete(Guid id)
    {
        if (!TryArtist(out var artistId)) return Forbid();
        var n = await _db.ClientNotes.FirstOrDefaultAsync(x => x.Id == id && x.ArtistId == artistId);
        if (n == null) return NotFound();
        n.IsDeleted = true;
        n.DeletedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Ok(new { deleted = true });
    }
}
