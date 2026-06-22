using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using SalonOS.Shared.Authorization;
using SalonOS.Shared;
using SalonOS.Shared.Identity;
using SalonOS.Infrastructure;

namespace SalonOS.Api.Controllers;

[Route("api/artist/contract")]
[ApiController]
[Authorize]
public class ArtistContractController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ICurrentUser _currentUser;

    public ArtistContractController(AppDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    // GET my contract(s)
    [HttpGet("my")]
    [HasPermission(Permissions.ArtistContractView)]
    public async Task<IActionResult> MyContracts()
    {
        var artistId = Guid.Parse(_currentUser.UserId);
        var contracts = await _db.ArtistContracts
            .Where(c => c.ArtistId == artistId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
        return Ok(contracts);
    }

    // POST create contract (manager only - not exposed to artist, but endpoint exists)
    [HttpPost]
    [HasPermission(Permissions.ArtistContractManage)]
    public async Task<IActionResult> Create([FromBody] ArtistContract contract)
    {
        _db.ArtistContracts.Add(contract);
        await _db.SaveChangesAsync();
        return Ok(contract);
    }
}