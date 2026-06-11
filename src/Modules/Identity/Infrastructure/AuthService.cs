using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SalonOS.Identity.Application.DTOs;
using SalonOS.Identity.Domain;

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
/// </summary>
public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IConfiguration _config;

    public AuthService(UserManager<ApplicationUser> userManager, IConfiguration config)
    {
        _userManager = userManager;
        _config = config;
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

        return new AuthResponseDto
        {
            Token = BuildToken(user),
            ExpiresIn = 30 * 24 * 60,
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

        return new AuthResponseDto
        {
            Token = BuildToken(user),
            ExpiresIn = 30 * 24 * 60,
            User = MapToProfile(user)
        };
    }

    public async Task<UserProfileDto?> GetProfileAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        return user == null ? null : MapToProfile(user);
    }

    public async Task<(bool Success, string Message)> ChangePasswordAsync(string userId, ChangePasswordDto dto)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return (false, "User not found");

        var result = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return (false, errors);
        }

        return (true, "Password changed successfully");
    }

    private string BuildToken(ApplicationUser user)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Name, user.UserName ?? ""),
            new Claim("UserType", user.UserType.ToString()),
            // TenantId will be added when user selects active tenant
            // This is handled by the TenantContextMiddleware
        };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_config["JwtSettings:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _config["JwtSettings:Issuer"],
            audience: _config["JwtSettings:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddDays(30),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static UserProfileDto MapToProfile(ApplicationUser user) => new()
    {
        Id = user.Id,
        FirstName = user.FirstName,
        LastName = user.LastName,
        Mobile = user.PhoneNumber ?? "",
        UserType = user.UserType.ToString(),
        LoyaltyPoints = user.LoyaltyPoints,
        TotalVisits = user.TotalVisits,
        IsActive = user.IsActive
    };
}
