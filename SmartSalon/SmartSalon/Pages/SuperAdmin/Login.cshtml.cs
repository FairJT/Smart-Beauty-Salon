using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SmartSalon.Models;

namespace SmartSalon.Pages.SuperAdmin
{
    public class LoginModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public string? ErrorMessage { get; set; }

        public LoginModel(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
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

            if (user.UserType != UserType.SuperAdmin)
            {
                ErrorMessage = "شما دسترسی به این پنل ندارید";
                return Page();
            }

            var passwordOk = await _userManager.CheckPasswordAsync(user, password);
            if (!passwordOk)
            {
                ErrorMessage = "رمز عبور اشتباه است";
                return Page();
            }

            HttpContext.Session.SetString("SuperAdminToken", user.Id);
            HttpContext.Session.SetString("SuperAdminName", user.FirstName + " " + user.LastName);

            return RedirectToPage("/SuperAdmin/Dashboard");
        }
    }
}