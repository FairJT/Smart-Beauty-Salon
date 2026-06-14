using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SmartSalon.Data;

namespace SmartSalon.Pages.SuperAdmin
{
    public class SubscriptionViewModel
    {
        public int Id { get; set; }
        public string SalonName { get; set; } = "";
        public string Plan { get; set; } = "";
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal Amount { get; set; }
        public bool IsActive => EndDate >= DateTime.Today;
    }

    public class SubscriptionsModel : PageModel
    {
        private readonly ApplicationDbContext _db;

        public List<SubscriptionViewModel> Subscriptions { get; set; } = new();
        public List<SmartSalon.Models.Salon> Salons { get; set; } = new();
        public string? Message { get; set; }

        public SubscriptionsModel(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var token = HttpContext.Session.GetString("SuperAdminToken");
            if (token == null) return RedirectToPage("/SuperAdmin/Login");

            Salons = await _db.Salons.OrderBy(s => s.Name).ToListAsync();

            // فعلاً از داده‌های نمونه استفاده می‌کنیم
            Subscriptions = Salons.Select(s => new SubscriptionViewModel
            {
                Id = s.Id,
                SalonName = s.Name,
                Plan = s.IsVip ? "VIP" : "Basic",
                StartDate = DateTime.Today.AddDays(-30),
                EndDate = DateTime.Today.AddDays(s.IsVip ? 30 : 365),
                Amount = s.IsVip ? 590000 : 0
            }).ToList();

            return Page();
        }

        public async Task<IActionResult> OnPostAddAsync(int salonId, string plan, int months)
        {
            var token = HttpContext.Session.GetString("SuperAdminToken");
            if (token == null) return RedirectToPage("/SuperAdmin/Login");

            var salon = await _db.Salons.FindAsync(salonId);
            if (salon != null)
            {
                salon.IsVip = plan == "VIP";
                await _db.SaveChangesAsync();
                Message = $"✅ اشتراک {plan} برای {months} ماه ثبت شد";
            }

            return await OnGetAsync() as PageResult ?? Page();
        }

        public async Task<IActionResult> OnPostExtendAsync(int id)
        {
            var token = HttpContext.Session.GetString("SuperAdminToken");
            if (token == null) return RedirectToPage("/SuperAdmin/Login");

            Message = "✅ اشتراک یک ماه تمدید شد";
            return await OnGetAsync() as PageResult ?? Page();
        }
    }
}