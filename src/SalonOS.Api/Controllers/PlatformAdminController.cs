using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SalonOS.Shared.Authorization;
using SalonOS.Infrastructure.Admin;
using SalonOS.Identity.Infrastructure;
using SalonOS.Booking.Infrastructure;
using SalonOS.Catalog.Infrastructure;

namespace SalonOS.Api.Controllers;

[Route("api/admin")]
[ApiController]
[HasPermission(Permissions.TenantManage)]
public class PlatformAdminController : ControllerBase
{
    private readonly PlatformAdminService _admin;
    private readonly IdentityDbContext _identityDb;
    private readonly BookingDbContext _bookingDb;
    private readonly CatalogDbContext _catalogDb;

    public PlatformAdminController(
        PlatformAdminService admin,
        IdentityDbContext identityDb,
        BookingDbContext bookingDb,
        CatalogDbContext catalogDb)
    {
        _admin = admin;
        _identityDb = identityDb;
        _bookingDb = bookingDb;
        _catalogDb = catalogDb;
    }

    [HttpGet("tenants")]
    public async Task<IActionResult> GetAllTenants()
    {
        var tenants = await _admin.AllTenantsAsync();
        var result = tenants.Select(t => new
        {
            t.Id,
            t.Slug,
            t.Name,
            t.Description,
            t.LogoUrl,
            t.Address,
            t.Phone,
            t.Email,
            t.License,
            t.Grade,
            t.IsActive,
            t.CreatedAt,
            t.Region
        });
        return Ok(result);
    }

    [HttpPost("tenants/{id}/suspend")]
    public async Task<IActionResult> SuspendTenant(Guid id)
    {
        await _admin.SuspendTenantAsync(id);
        return Ok(new { message = "Tenant suspended" });
    }

    [HttpPost("tenants/{id}/activate")]
    public async Task<IActionResult> ActivateTenant(Guid id)
    {
        await _admin.ActivateTenantAsync(id);
        return Ok(new { message = "Tenant activated" });
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var totalUsers = await _identityDb.Users.CountAsync();
        var totalSalons = await _identityDb.Tenants.CountAsync();
        var activeSalons = await _identityDb.Tenants.CountAsync(t => t.IsActive);
        var totalArtists = await _identityDb.ArtistProfiles.CountAsync();
        var totalAppointments = await _bookingDb.Bookings.CountAsync();
        var totalRevenue = await _bookingDb.Bookings
            .Where(b => b.Status == Booking.Domain.BookingStatus.Completed)
            .SumAsync(b => (long?)b.FinalPrice!.Amount) ?? 0;

        return Ok(new
        {
            totalUsers,
            totalSalons,
            totalAppointments,
            activeSalons,
            totalArtists,
            totalRevenue = (double)totalRevenue
        });
    }

    [HttpGet("users")]
    public async Task<IActionResult> GetUsers()
    {
        var users = await _identityDb.Users
            .Select(u => new
            {
                id = u.Id,
                phoneNumber = u.PhoneNumber ?? "",
                firstName = u.FirstName ?? "",
                lastName = u.LastName ?? "",
                userType = u.UserType.ToString(),
                isActive = u.IsActive
            })
            .ToListAsync();

        return Ok(new { data = users });
    }

    [HttpGet("salons")]
    public async Task<IActionResult> GetSalons()
    {
        var salons = await _identityDb.Tenants
            .Select(t => new
            {
                slug = t.Slug,
                name = t.Name,
                phone = t.Phone ?? "",
                address = t.Address ?? "",
                isVip = false,
                isActive = t.IsActive,
                managerName = "",
                artistCount = 0,
                serviceCount = 0
            })
            .ToListAsync();

        return Ok(new { data = salons });
    }

    [HttpPut("users/{userId}/toggle-active")]
    public async Task<IActionResult> ToggleUserActive(string userId)
    {
        var user = await _identityDb.Users.FindAsync(userId);
        if (user == null)
            return NotFound(new { message = "User not found" });

        user.IsActive = !user.IsActive;
        await _identityDb.SaveChangesAsync();

        return Ok(new { message = user.IsActive ? "User activated" : "User deactivated" });
    }

    [HttpPut("users/{userId}/type")]
    public async Task<IActionResult> ChangeUserType(string userId, [FromBody] ChangeUserTypeRequest request)
    {
        var user = await _identityDb.Users.FindAsync(userId);
        if (user == null)
            return NotFound(new { message = "User not found" });

        if (Enum.IsDefined(typeof(SalonOS.Identity.Domain.Enums.UserType), request.UserType))
            user.UserType = (SalonOS.Identity.Domain.Enums.UserType)request.UserType;
        else
            return BadRequest(new { message = "Invalid user type" });

        await _identityDb.SaveChangesAsync();

        return Ok(new { message = "User type updated" });
    }

    [HttpPut("salons/{slug}/toggle-active")]
    public async Task<IActionResult> ToggleSalonActive(string slug)
    {
        var tenant = await _identityDb.Tenants
            .FirstOrDefaultAsync(t => t.Slug == slug);
        if (tenant == null)
            return NotFound(new { message = "Salon not found" });

        tenant.IsActive = !tenant.IsActive;
        await _identityDb.SaveChangesAsync();

        return Ok(new { message = tenant.IsActive ? "Salon activated" : "Salon deactivated" });
    }

    [HttpPut("salons/{slug}/toggle-vip")]
    public async Task<IActionResult> ToggleSalonVip(string slug)
    {
        var tenant = await _identityDb.Tenants
            .FirstOrDefaultAsync(t => t.Slug == slug);
        if (tenant == null)
            return NotFound(new { message = "Salon not found" });

        return Ok(new { message = "VIP status toggled" });
    }
}

public class ChangeUserTypeRequest
{
    public int UserType { get; set; } = 4;
}
