using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using SmartSalon.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SmartSalon.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _config;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            IConfiguration config)
        {
            _userManager = userManager;
            _config = config;
        }

        // ─── ثبت‌نام ──────────────────────────────────────────
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            if (await _userManager.FindByNameAsync(dto.Mobile) != null)
                return BadRequest(new { message = "این شماره موبایل قبلاً ثبت شده است" });

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
                return BadRequest(new { errors = result.Errors.Select(e => e.Description) });

            return Ok(new { message = "ثبت‌نام موفق بود", userId = user.Id });
        }

        // ─── ورود ─────────────────────────────────────────────
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var user = await _userManager.FindByNameAsync(dto.Mobile);

            if (user == null || !user.IsActive)
                return Unauthorized(new { message = "کاربر یافت نشد" });

            var passwordOk = await _userManager.CheckPasswordAsync(user, dto.Password);

            if (!passwordOk)
                return Unauthorized(new { message = "رمز عبور اشتباه است" });

            var token = BuildToken(user);

            return Ok(new
            {
                token,
                expiresIn = 30 * 24 * 60,
                user = new
                {
                    id = user.Id,
                    firstName = user.FirstName,
                    lastName = user.LastName,
                    mobile = user.PhoneNumber,
                    userType = user.UserType.ToString()
                }
            });
        }

        // ─── پروفایل من ───────────────────────────────────────
        [HttpGet("profile")]
        [Authorize]
        public async Task<IActionResult> GetProfile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId!);

            if (user == null)
                return NotFound(new { message = "کاربر یافت نشد" });

            return Ok(new
            {
                id = user.Id,
                firstName = user.FirstName,
                lastName = user.LastName,
                mobile = user.PhoneNumber,
                nationalCode = user.NationalCode,
                userType = user.UserType.ToString(),
                loyaltyPoints = user.LoyaltyPoints,
                totalVisits = user.TotalVisits,
                isActive = user.IsActive
            });
        }

        // ─── ساخت توکن JWT ────────────────────────────────────
        private string BuildToken(ApplicationUser user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName ?? ""),
                new Claim("UserType", user.UserType.ToString()),
            };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config["JwtSettings:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["JwtSettings:Issuer"],
                audience: _config["JwtSettings:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddDays(30),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }

    // ─── DTOs ─────────────────────────────────────────────────
    public record RegisterDto(
        string Mobile,
        string Password,
        string FirstName,
        string LastName,
        string NationalCode
    );

    public record LoginDto(string Mobile, string Password);
}