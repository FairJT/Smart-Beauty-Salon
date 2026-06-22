# A2 — Customer notes / suggestions / product tips 🟡

One entity for items 10, 11, 14 — the artist's private notes about a client.

## Step 1 — entity
**New file:** `src/SalonOS.Infrastructure/SalonMgmt/ClientNote.cs`
```csharp
using SalonOS.Shared;

namespace SalonOS.Infrastructure;

public enum ClientNoteType { Preference = 1, Sensitivity = 2, Suggestion = 3, ProductTip = 4 }

public class ClientNote : TenantEntity
{
    public Guid ArtistId { get; set; }
    public string ClientId { get; set; } = string.Empty;
    public ClientNoteType Type { get; set; } = ClientNoteType.Preference;
    public string Text { get; set; } = string.Empty;
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
    public DbSet<ClientNote> ClientNotes { get; set; }
```

## Step 3 — migration
```powershell
dotnet ef migrations add ClientNotes --project src\SalonOS.Infrastructure --startup-project src\SalonOS.Api --context AppDbContext
```

## Step 4 — controller
**New file:** `src/SalonOS.Api/Controllers/ClientNoteController.cs`
```csharp
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using SalonOS.Shared.Authorization;
using SalonOS.Infrastructure;

namespace SalonOS.Api.Controllers;

[Route("api/client-notes")]
[ApiController]
[Authorize]
public class ClientNoteController : ControllerBase
{
    private readonly AppDbContext _db;
    public ClientNoteController(AppDbContext db) => _db = db;

    public record NoteRequest(string ClientId, ClientNoteType Type, string Text);

    private bool TryArtist(out Guid artistId)
    {
        artistId = Guid.Empty;
        var v = User.FindFirst("artist_id")?.Value;
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
        var n = new ClientNote { ArtistId = artistId, ClientId = r.ClientId, Type = r.Type, Text = r.Text };
        _db.ClientNotes.Add(n);
        await _db.SaveChangesAsync();
        return Ok(n);
    }

    [HttpDelete("{id}")]
    [HasPermission(Permissions.ClientNoteManageOwn)]
    public async Task<IActionResult> Delete(Guid id)
    {
        if (!TryArtist(out var artistId)) return Forbid();
        var n = await _db.ClientNotes.FirstOrDefaultAsync(x => x.Id == id && x.ArtistId == artistId);
        if (n is null) return NotFound();
        n.IsDeleted = true; n.DeletedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Ok(new { deleted = true });
    }
}
```

**Done when:** build succeeds; an artist can create/list/delete their own client notes.
