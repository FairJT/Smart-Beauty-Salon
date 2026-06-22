using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using SalonOS.Shared.Authorization;
using SalonOS.Shared;
using SalonOS.Shared.Identity;
using SalonOS.Infrastructure;

namespace SalonOS.Api.Controllers;

[Route("api/artist/leave")]
[ApiController]
[Authorize]
public class ArtistLeaveController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ICurrentUser _currentUser;

    public ArtistLeaveController(AppDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    // GET my leaves
    [HttpGet("my")]
    [HasPermission(Permissions.ArtistLeaveView)]
    public async Task<IActionResult> MyLeaves()
    {
        var artistId = Guid.Parse(_currentUser.UserId); // Assuming UserId holds artist GUID
        var leaves = await _db.ArtistLeaves
            .Where(l => l.ArtistId == artistId)
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync();
        return Ok(leaves);
    }

    // POST request leave
    [HttpPost]
    [HasPermission(Permissions.ArtistLeaveManage)]
    public async Task<IActionResult> RequestLeave([FromBody] ArtistLeave request)
    {
        var artistId = Guid.Parse(_currentUser.UserId);
        request.ArtistId = artistId;
        _db.ArtistLeaves.Add(request);
        await _db.SaveChangesAsync();
        return Ok(request);
    }

    // PUT update status (manager only - not exposed to artist)
}