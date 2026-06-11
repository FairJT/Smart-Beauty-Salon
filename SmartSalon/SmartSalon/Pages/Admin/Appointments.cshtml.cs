using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SmartSalon.Data;
using SmartSalon.Models;

namespace SmartSalon.Pages.Admin
{
    public class AppointmentsModel : PageModel
    {
        private readonly ApplicationDbContext _db;

        public List<Appointment> Appointments { get; set; } = new();
        public decimal TodayRevenue { get; set; }
        public DateTime SelectedDate { get; set; } = DateTime.Today;
        public int? SalonId { get; set; }

        public AppointmentsModel(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> OnGetAsync(DateTime? date)
        {
            var token = HttpContext.Session.GetString("AdminToken");
            if (token == null) return RedirectToPage("/Admin/Login");

            // پیدا کردن سالن مرتبط با این مدیر
            var salon = await _db.Salons
                .FirstOrDefaultAsync(s => s.ManagerId == token);

            if (salon == null)
            {
                Appointments = new();
                return Page();
            }

            SalonId = salon.Id;
            SelectedDate = date?.Date ?? DateTime.Today;

            Appointments = await _db.Appointments
                .Where(a => a.StartTime.Date == SelectedDate && a.SalonId == salon.Id)
                .Include(a => a.Client)
                .Include(a => a.Artist).ThenInclude(ar => ar!.User)
                .Include(a => a.Service)
                .OrderBy(a => a.StartTime)
                .ToListAsync();

            TodayRevenue = Appointments
                .Where(a => a.Status != AppointmentStatus.Cancelled)
                .Sum(a => a.EstimatedPrice);

            return Page();
        }

        public async Task<IActionResult> OnPostConfirmAsync(int id)
        {
            var token = HttpContext.Session.GetString("AdminToken");
            if (token == null) return RedirectToPage("/Admin/Login");

            var a = await _db.Appointments
                .Include(a => a.Salon)
                .FirstOrDefaultAsync(a => a.Id == id && a.Salon!.ManagerId == token);

            if (a != null)
            {
                a.Status = AppointmentStatus.Confirmed;
                await _db.SaveChangesAsync();
            }
            return RedirectToPage(new { date = Request.Query["date"] });
        }

        public async Task<IActionResult> OnPostCancelAsync(int id)
        {
            var token = HttpContext.Session.GetString("AdminToken");
            if (token == null) return RedirectToPage("/Admin/Login");

            var a = await _db.Appointments
                .Include(a => a.Salon)
                .FirstOrDefaultAsync(a => a.Id == id && a.Salon!.ManagerId == token);

            if (a != null)
            {
                a.Status = AppointmentStatus.Cancelled;
                await _db.SaveChangesAsync();
            }
            return RedirectToPage(new { date = Request.Query["date"] });
        }

        public async Task<IActionResult> OnPostCompleteAsync(int id)
        {
            var token = HttpContext.Session.GetString("AdminToken");
            if (token == null) return RedirectToPage("/Admin/Login");

            var a = await _db.Appointments
                .Include(a => a.Salon)
                .FirstOrDefaultAsync(a => a.Id == id && a.Salon!.ManagerId == token);

            if (a != null)
            {
                a.Status = AppointmentStatus.Completed;
                await _db.SaveChangesAsync();
            }
            return RedirectToPage(new { date = Request.Query["date"] });
        }
    }
}