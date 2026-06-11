using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SmartSalon.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SmartSalon.Pages.Admin
{
    public class LoginModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _config;

        public string? ErrorMessage { get; set; }

        public LoginModel(UserManager<ApplicationUser> userManager, IConfiguration config)
        {
            _userManager = userManager;
            _config = config;
        }

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync(string mobile, string password)
        {
            var user = await _userManager.FindByNameAsync(mobile);

            if (user == null || !user.IsActive)
            {
                ErrorMessage = "کاربر یافت نشد";
                return Page();
            }

            if (user.UserType != UserType.SalonManager && user.UserType != UserType.SuperAdmin)
            {
                ErrorMessage = "شما دسترسی به پنل مدیریت ندارید";
                return Page();
            }

            var passwordOk = await _userManager.CheckPasswordAsync(user, password);
            if (!passwordOk)
            {
                ErrorMessage = "رمز عبور اشتباه است";
                return Page();
            }

            // ذخیره توکن در Session
            var token = BuildToken(user);
            HttpContext.Session.SetString("AdminToken", user.Id);
            HttpContext.Session.SetString("AdminName", user.FirstName + " " + user.LastName);

            return RedirectToPage("/Admin/Dashboard");
        }

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
                expires: DateTime.UtcNow.AddDays(1),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}