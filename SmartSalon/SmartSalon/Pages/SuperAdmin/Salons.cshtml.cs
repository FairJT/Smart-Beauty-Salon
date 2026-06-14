using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SmartSalon.Data;
using SmartSalon.Models;

namespace SmartSalon.Pages.SuperAdmin
{
    public class SalonsModel : PageModel
    {
        private readonly ApplicationDbContext _db;

        public List<SmartSalon.Models.Salon> Salons { get; set; } = new();
        public string? Message { get; set; }

        public SalonsModel(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var token = HttpContext.Session.GetString("SuperAdminToken");
            if (token == null) return RedirectToPage("/SuperAdmin/Login");

            Salons = await _db.Salons
                .OrderByDescending(s => s.Id)
                .ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostAddAsync(
            string name, string slug, string managerMobile,
            string? address, string? phone)
        {
            var token = HttpContext.Session.GetString("SuperAdminToken");
            if (token == null) return RedirectToPage("/SuperAdmin/Login");

            var manager = await _db.Users
                .FirstOrDefaultAsync(u => u.UserName == managerMobile);

            if (manager == null)
            {
                Message = "❌ مدیری با این موبایل یافت نشد";
                return await OnGetAsync() as PageResult ?? Page();
            }

            // چک کنیم slug تکراری نباشد
            if (await _db.Salons.AnyAsync(s => s.Slug == slug))
            {
                Message = "❌ این آدرس URL قبلاً استفاده شده";
                return await OnGetAsync() as PageResult ?? Page();
            }

            var salon = new SmartSalon.Models.Salon
            {
                Name = name,
                Slug = slug,
                Address = address,
                Phone = phone,
                ThemeColor = "#1B3A5C",
                ManagerId = manager.Id  // ← وصل کردن مدیر به سالن
            };

            _db.Salons.Add(salon);
            await _db.SaveChangesAsync();

            // تغییر نقش کاربر به مدیر سالن
            manager.UserType = UserType.SalonManager;
            await _db.SaveChangesAsync();

            Message = "✅ سالن با موفقیت اضافه شد و مدیر وصل شد";
            return await OnGetAsync() as PageResult ?? Page();
        }

        public async Task<IActionResult> OnPostToggleAsync(int id)
        {
            var token = HttpContext.Session.GetString("SuperAdminToken");
            if (token == null) return RedirectToPage("/SuperAdmin/Login");

            var salon = await _db.Salons.FindAsync(id);
            if (salon != null)
            {
                salon.IsActive = !salon.IsActive;
                await _db.SaveChangesAsync();
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostToggleVipAsync(int id)
        {
            var token = HttpContext.Session.GetString("SuperAdminToken");
            if (token == null) return RedirectToPage("/SuperAdmin/Login");

            var salon = await _db.Salons.FindAsync(id);
            if (salon != null)
            {
                salon.IsVip = !salon.IsVip;
                await _db.SaveChangesAsync();
            }
            return RedirectToPage();
        }
    }
}