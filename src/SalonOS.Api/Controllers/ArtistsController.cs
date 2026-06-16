using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using SalonOS.Identity.Domain;
using SalonOS.Identity.Infrastructure;
using SalonOS.Booking.Infrastructure;
using SalonOS.Shared;

namespace SalonOS.Api.Controllers;

[Route("api/artists")]
[ApiController]
[Authorize]
public class ArtistsController : ControllerBase
{
    private readonly IdentityDbContext _identityDb;
    private readonly BookingDbContext _bookingDb;
    private readonly ITenantContext _tenant;

    public ArtistsController(IdentityDbContext identityDb, BookingDbContext bookingDb, ITenantContext tenant)
    {
        _identityDb = identityDb;
        _bookingDb = bookingDb;
        _tenant = tenant;
    }

    [HttpGet]
    public async Task<IActionResult> GetArtists()
    {
        var artists = await _identityDb.ArtistProfiles
            .Where(a => a.IsActive)
            .Join(_identityDb.Users,
                profile => profile.UserId,
                user => user.Id,
                (profile, user) => new
                {
                    id = profile.Id,
                    firstName = user.FirstName ?? "",
                    lastName = user.LastName ?? "",
                    phoneNumber = user.PhoneNumber ?? "",
                    isActive = profile.IsActive,
                    rating = 0.0,
                    totalAppointments = 0
                })
            .ToListAsync();

        return Ok(artists);
    }

    [HttpGet("salon/{slug}")]
    public async Task<IActionResult> GetArtistsBySalon(string slug)
    {
        var tenant = await _identityDb.Tenants
            .Where(t => t.Slug == slug && t.IsActive)
            .Select(t => t.Id)
            .FirstOrDefaultAsync();

        if (tenant == Guid.Empty)
            return NotFound(new { message = "Salon not found" });

        var artists = await _identityDb.ArtistProfiles
            .Where(a => a.TenantId == tenant && a.IsActive)
            .Join(_identityDb.Users,
                profile => profile.UserId,
                user => user.Id,
                (profile, user) => new
                {
                    id = profile.Id,
                    name = $"{user.FirstName} {user.LastName}".Trim(),
                    phoneNumber = user.PhoneNumber ?? "",
                    profileImageUrl = (string?)null,
                    specialization = profile.Skill ?? "",
                    isActive = profile.IsActive
                })
            .ToListAsync();

        return Ok(artists);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetArtist(Guid id)
    {
        var artist = await _identityDb.ArtistProfiles
            .Where(a => a.Id == id)
            .Join(_identityDb.Users,
                profile => profile.UserId,
                user => user.Id,
                (profile, user) => new
                {
                    id = profile.Id,
                    name = $"{user.FirstName} {user.LastName}".Trim(),
                    phoneNumber = user.PhoneNumber ?? "",
                    profileImageUrl = (string?)null,
                    specialization = profile.Skill ?? "",
                    isActive = profile.IsActive
                })
            .FirstOrDefaultAsync();

        if (artist == null)
            return NotFound(new { message = "Artist not found" });

        return Ok(artist);
    }

    [HttpPost]
    public async Task<IActionResult> CreateArtist([FromBody] CreateArtistRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        Tenant? tenant;
        if (!string.IsNullOrWhiteSpace(request.Slug))
        {
            tenant = await _identityDb.Tenants
                .Where(t => t.Slug == request.Slug && t.IsActive)
                .FirstOrDefaultAsync();
        }
        else
        {
            tenant = await _identityDb.Tenants
                .Where(t => t.Id == _tenant.TenantId && t.IsActive)
                .FirstOrDefaultAsync();
        }

        if (tenant == null)
            return NotFound(new { message = "Salon not found" });

        var user = new Identity.Domain.ApplicationUser
        {
            UserName = request.PhoneNumber,
            PhoneNumber = request.PhoneNumber,
            FirstName = request.FirstName,
            LastName = request.LastName,
            IsActive = true
        };

        var result = await _identityDb.Users.AddAsync(user);
        await _identityDb.SaveChangesAsync();

        var profile = new Identity.Domain.ArtistProfile
        {
            UserId = user.Id,
            TenantId = tenant.Id,
            Skill = request.Specialization ?? "",
            IsActive = true
        };

        await _identityDb.ArtistProfiles.AddAsync(profile);
        await _identityDb.SaveChangesAsync();

        return CreatedAtAction(nameof(GetArtist), new { id = profile.Id }, new
        {
            id = profile.Id,
            name = $"{request.FirstName} {request.LastName}".Trim(),
            phoneNumber = request.PhoneNumber,
            profileImageUrl = (string?)null,
            specialization = request.Specialization,
            isActive = true
        });
    }

    [HttpPut("{id}/toggle-active")]
    public async Task<IActionResult> ToggleActive(Guid id)
    {
        var profile = await _identityDb.ArtistProfiles.FindAsync(id);
        if (profile == null)
            return NotFound(new { message = "Artist not found" });

        profile.IsActive = !profile.IsActive;
        await _identityDb.SaveChangesAsync();

        return Ok(new { message = profile.IsActive ? "Artist activated" : "Artist deactivated" });
    }
}

public class CreateArtistRequest
{
    public string? Slug { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string? Specialization { get; set; }
}
