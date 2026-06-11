using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SmartSalon.Data;

namespace SmartSalon.Pages.Admin
{
    public class SettingsModel : PageModel
    {
        private readonly ApplicationDbContext _db;

        public SmartSalon.Models.Salon? Salon { get; set; }
        public string? Message { get; set; }

        public SettingsModel(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var token = HttpContext.Session.GetString("AdminToken");
            if (token == null) return RedirectToPage("/Admin/Login");

            // فقط سالن مرتبط با این مدیر
            Salon = await _db.Salons
                .FirstOrDefaultAsync(s => s.ManagerId == token);

            if (Salon == null)
                Message = "سالنی برای این مدیر یافت نشد";

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(
            string name, string slug, string? phone,
            string? address, string? description, string themeColor)
        {
            var token = HttpContext.Session.GetString("AdminToken");
            if (token == null) return RedirectToPage("/Admin/Login");

            // فقط سالن مرتبط با این مدیر
            Salon = await _db.Salons
                .FirstOrDefaultAsync(s => s.ManagerId == token);

            if (Salon == null)
            {
                Message = "سالنی برای این مدیر یافت نشد";
                return Page();
            }

            Salon.Name = name;
            Salon.Slug = slug;
            Salon.Phone = phone;
            Salon.Address = address;
            Salon.Description = description;
            Salon.ThemeColor = themeColor;

            await _db.SaveChangesAsync();
            Message = "✅ تنظیمات با موفقیت ذخیره شد";
            return Page();
        }

        public async Task<IActionResult> OnPostSetThemeAsync(string themeName)
        {
            var token = HttpContext.Session.GetString("AdminToken");
            if (token == null) return RedirectToPage("/Admin/Login");

            Salon = await _db.Salons.FirstOrDefaultAsync(s => s.ManagerId == token);
            if (Salon != null)
            {
                Salon.AdminTheme = themeName;
                await _db.SaveChangesAsync();
                Message = "✅ تم با موفقیت تغییر کرد";
            }
            return RedirectToPage();
        }
    }
}