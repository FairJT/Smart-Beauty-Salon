using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SmartSalon.Data;
using SmartSalon.Models;

namespace SmartSalon.Pages.Admin
{
    public class ServicesModel : PageModel
    {
        private readonly ApplicationDbContext _db;

        public List<SalonService> Services { get; set; } = new();
        public string? Message { get; set; }

        public ServicesModel(ApplicationDbContext db)
        {
            _db = db;
        }

        private async Task<SmartSalon.Models.Salon?> GetManagerSalonAsync()
        {
            var token = HttpContext.Session.GetString("AdminToken");
            if (token == null) return null;
            return await _db.Salons.FirstOrDefaultAsync(s => s.ManagerId == token);
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var token = HttpContext.Session.GetString("AdminToken");
            if (token == null) return RedirectToPage("/Admin/Login");

            var salon = await GetManagerSalonAsync();
            if (salon == null)
            {
                Message = "سالنی برای این مدیر یافت نشد";
                return Page();
            }

            Services = await _db.SalonServices
                .Where(s => s.SalonId == salon.Id)
                .OrderBy(s => s.Category)
                .ThenBy(s => s.Name)
                .ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostAddAsync(
            string name, string category, int duration, decimal price)
        {
            var token = HttpContext.Session.GetString("AdminToken");
            if (token == null) return RedirectToPage("/Admin/Login");

            var salon = await GetManagerSalonAsync();
            if (salon == null)
            {
                Message = "سالنی برای این مدیر یافت نشد";
                Services = new();
                return Page();
            }

            _db.SalonServices.Add(new SalonService
            {
                Name = name,
                Category = category,
                BaseDurationMinutes = duration,
                BasePrice = price,
                SalonId = salon.Id
            });
            await _db.SaveChangesAsync();

            Message = "✅ خدمت با موفقیت اضافه شد";
            Services = await _db.SalonServices
                .Where(s => s.SalonId == salon.Id)
                .OrderBy(s => s.Category)
                .ThenBy(s => s.Name)
                .ToListAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostToggleAsync(int id)
        {
            var token = HttpContext.Session.GetString("AdminToken");
            if (token == null) return RedirectToPage("/Admin/Login");

            var salon = await GetManagerSalonAsync();
            if (salon == null) return RedirectToPage();

            var service = await _db.SalonServices
                .FirstOrDefaultAsync(s => s.Id == id && s.SalonId == salon.Id);

            if (service != null)
            {
                service.IsActive = !service.IsActive;
                await _db.SaveChangesAsync();
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var token = HttpContext.Session.GetString("AdminToken");
            if (token == null) return RedirectToPage("/Admin/Login");

            var salon = await GetManagerSalonAsync();
            if (salon == null) return RedirectToPage();

            var service = await _db.SalonServices
                .FirstOrDefaultAsync(s => s.Id == id && s.SalonId == salon.Id);

            if (service != null)
            {
                service.IsActive = false;
                await _db.SaveChangesAsync();
            }
            return RedirectToPage();
        }
    }
}