using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SalonOS.Identity.Application.DTOs;
using SalonOS.Identity.Infrastructure;
using System.Security.Claims;

namespace SalonOS.Identity.API.Controllers;

/// <summary>
/// Authentication controller.
/// Task 6.1: Register and Login are [AllowAnonymous]; all other actions require [Authorize].
/// Auth actions don't carry resource-level permissions — they operate on the caller's
/// own identity so plain [Authorize] (valid JWT) is the correct gate here.
/// </summary>
[Route("api/auth")]
[ApiController]
[Authorize]
[Authorize]                 // default: every action requires a valid JWT …
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;

    public AuthController(IAuthService auth) => _auth = auth;

    // ── Public endpoints ──────────────────────────────────────────────────────

    [HttpPost("register")]
    [AllowAnonymous]        // … except register …
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _auth.RegisterAsync(dto);
        if (result == null)
            return BadRequest(new { message = "This mobile number is already registered" });

        return Ok(result);
    }

    [HttpPost("login")]
    [AllowAnonymous]        // … and login.
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _auth.LoginAsync(dto);
        if (result == null)
            return Unauthorized(new { message = "Invalid mobile number or password" });

        return Ok(result);
    }

    // ── Authenticated endpoints ───────────────────────────────────────────────

    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();

        var profile = await _auth.GetProfileAsync(userId);
        if (profile == null) return NotFound(new { message = "User not found" });

        return Ok(profile);
    }

    [HttpPost("logout")]
    public IActionResult Logout() => Ok(new { message = "Logged out successfully" });

    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();

        var (success, message) = await _auth.ChangePasswordAsync(userId, dto);
        if (!success)
            return BadRequest(new { message });

        return Ok(new { message = "Password changed successfully" });
    }
}
