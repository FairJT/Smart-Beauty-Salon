using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SalonOS.Identity.Application.DTOs;
using SalonOS.Identity.Domain;
using SalonOS.Identity.Domain.Enums;
using SalonOS.Shared.Authorization;

namespace SalonOS.Identity.Infrastructure;

/// <summary>
/// Interface for authentication service.
/// </summary>
public interface IAuthService
{
    Task<AuthResponseDto?> RegisterAsync(RegisterDto dto);
    Task<AuthResponseDto?> LoginAsync(LoginDto dto);
    Task<UserProfileDto?> GetProfileAsync(string userId);
    Task<(bool Success, string Message)> ChangePasswordAsync(string userId, ChangePasswordDto dto);
}

/// <summary>
/// Authentication service implementation.
/// Handles user registration, login, and JWT token generation.
///
/// Task 3.2: the token now embeds:
///   - one "permission" claim per permission in RolePermissions.Map[role]
///   - "tenant_id" (from the user's active membership)
///   - "role"      (SalonManager / Receptionist / Artist / Client)
///   - "artist_id" (only for Artist role)
///   - "is_platform_owner" = "true" (only for SuperAdmin — no permission list)
///
/// Task 3.3: access token lifetime = 30 minutes (was 30 days).
/// </summary>
public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IConfiguration _config;
    private readonly IdentityDbContext _db;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        IConfiguration config,
        IdentityDbContext db)
    {
        _userManager = userManager;
        _config = config;
        _db = db;
    }

    public async Task<AuthResponseDto?> RegisterAsync(RegisterDto dto)
    {
        if (await _userManager.FindByNameAsync(dto.Mobile) != null)
            return null;

        var user = new ApplicationUser
        {
            UserName = dto.Mobile,
            PhoneNumber = dto.Mobile,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            NationalCode = dto.NationalCode,
            UserType = UserType.Client
        };

        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
            return null;

        // Create a ClientProfile for new registrants
        _db.ClientProfiles.Add(new ClientProfile { UserId = user.Id });
        await _db.SaveChangesAsync();

        // New clients have no membership yet — token has no tenant/permissions until they join a tenant.
        var token = await BuildTokenAsync(user);
        return new AuthResponseDto
        {
            Token = token,
            ExpiresIn = 30,   // minutes
            User = MapToProfile(user)
        };
    }

    public async Task<AuthResponseDto?> LoginAsync(LoginDto dto)
    {
        var user = await _userManager.FindByNameAsync(dto.Mobile);
        if (user == null || !user.IsActive)
            return null;

        if (!await _userManager.CheckPasswordAsync(user, dto.Password))
            return null;

        var token = await BuildTokenAsync(user);
        return new AuthResponseDto
        {
            Token = token,
            ExpiresIn = 30,   // minutes
            User = MapToProfile(user)
        };
    }

    public async Task<UserProfileDto?> GetProfileAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        return user == null ? null : MapToProfile(user);
    }

    public async Task<(bool Success, string Message)> ChangePasswordAsync(
        string userId, ChangePasswordDto dto)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return (false, "User not found");

        var result = await _userManager.ChangePasswordAsync(
            user, dto.CurrentPassword, dto.NewPassword);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return (false, errors);
        }

        return (true, "Password changed successfully");
    }

    // ─── Token builder ────────────────────────────────────────────────────────

    private async Task<string> BuildTokenAsync(ApplicationUser user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name, user.UserName ?? string.Empty),
        };

        // PlatformOwner (SuperAdmin) — bypass permission list, set flag instead.
        if (user.UserType == UserType.SuperAdmin)
        {
            claims.Add(new Claim("is_platform_owner", "true"));
            claims.Add(new Claim("role", "PlatformOwner"));
        }
        else
        {
            // Map UserType → role name that matches RolePermissions.Map keys
            var roleName = user.UserType switch
            {
                UserType.SalonManager => "SalonManager",
                UserType.Artist       => "Artist",
                _                     => "Client"
            };

            // Fetch the user's active membership to get TenantId (and ArtistId if Artist)
            var membership = await _db.Memberships
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.UserId == user.Id && m.IsActive);

            if (membership != null)
            {
                claims.Add(new Claim("tenant_id", membership.TenantId.ToString()));

                // Receptionist has its own MembershipRole value (Task 7.2).
                if (membership.Role == MembershipRole.Receptionist)
                    roleName = "Receptionist";
            }

            claims.Add(new Claim("role", roleName));

            // Embed one "permission" claim per permission the role holds.
            if (RolePermissions.Map.TryGetValue(roleName, out var perms))
                foreach (var p in perms)
                    claims.Add(new Claim("permission", p));

            // Artist: embed artist_id from ArtistProfile so ownership checks can compare without a DB hit.
            if (user.UserType == UserType.Artist)
            {
                var artistProfile = await _db.ArtistProfiles
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.UserId == user.Id);
                if (artistProfile != null)
                    claims.Add(new Claim("artist_id", artistProfile.Id.ToString()));
            }
        }

        var jwtKey = _config["JwtSettings:Key"];
        if (string.IsNullOrEmpty(jwtKey))
            jwtKey = Environment.GetEnvironmentVariable("JWT_SECRET")
                ?? throw new InvalidOperationException("JWT key is not configured.");

        var key   = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer:            _config["JwtSettings:Issuer"],
            audience:          _config["JwtSettings:Audience"],
            claims:            claims,
            expires:           DateTime.UtcNow.AddMinutes(30),   // Task 3.3 — 30-min access token
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static UserProfileDto MapToProfile(ApplicationUser user) => new()
    {
        Id           = user.Id,
        FirstName    = user.FirstName,
        LastName     = user.LastName,
        Mobile       = user.PhoneNumber ?? string.Empty,
        UserType     = user.UserType.ToString(),
        LoyaltyPoints = user.LoyaltyPoints,
        TotalVisits  = user.TotalVisits,
        IsActive     = user.IsActive
    };
}
