using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SmartSalon.DTOs;
using SmartSalon.Models;
using SmartSalon.Services;
using System.Security.Claims;

namespace SmartSalon.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _auth;
        private readonly UserManager<ApplicationUser> _userManager;

        public AuthController(IAuthService auth, UserManager<ApplicationUser> userManager)
        {
            _auth = auth;
            _userManager = userManager;
        }

        [HttpPost("register")]
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
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _auth.LoginAsync(dto);
            if (result == null)
                return Unauthorized(new { message = "Invalid mobile number or password" });

            return Ok(result);
        }

        [HttpGet("profile")]
        [Authorize]
        public async Task<IActionResult> GetProfile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var profile = await _auth.GetProfileAsync(userId);
            if (profile == null) return NotFound(new { message = "User not found" });

            return Ok(profile);
        }

        [HttpPost("logout")]
        [Authorize]
        public IActionResult Logout()
        {
            return Ok(new { message = "Logged out successfully" });
        }

        // ── POST /api/auth/seed ────────────────────────────────────────
        [HttpPost("seed")]
        public async Task<IActionResult> Seed()
        {
            var results = new List<object>();

            var users = new[]
            {
                new { Mobile = "09110000001", Password = "Test@1234", FirstName = "مدیر", LastName = "سامانه", Type = UserType.SuperAdmin },
                new { Mobile = "09110000002", Password = "Test@1234", FirstName = "مدیر", LastName = "سالن", Type = UserType.SalonManager },
                new { Mobile = "09110000003", Password = "Test@1234", FirstName = "هنرمند", LastName = "نمونه", Type = UserType.Artist },
                new { Mobile = "09110000004", Password = "Test@1234", FirstName = "مشتری", LastName = "نمونه", Type = UserType.Client },
            };

            foreach (var u in users)
            {
                var existing = await _auth.RegisterAsync(new RegisterDto
                {
                    Mobile = u.Mobile,
                    Password = u.Password,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    NationalCode = "1234567890",
                });

                // Override user type for non-Client roles
                if (existing != null && u.Type != UserType.Client)
                {
                    var user = await _userManager.FindByIdAsync(existing.User.Id);
                    if (user != null)
                    {
                        user.UserType = u.Type;
                        await _userManager.UpdateAsync(user);
                    }
                }

                results.Add(new
                {
                    mobile = u.Mobile,
                    password = u.Password,
                    role = u.Type.ToString(),
                    status = existing != null ? "created" : "already exists",
                });
            }

            return Ok(new { message = "Seed complete", users = results });
        }

        [HttpPost("change-password")]
        [Authorize]
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
}
