using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SmartSalon.Data;
using SmartSalon.Models;

namespace SmartSalon.Pages.SuperAdmin
{
    public class DashboardModel : PageModel
    {
        private readonly ApplicationDbContext _db;

        public string AdminName { get; set; } = "";
        public int TotalSalons { get; set; }
        public int ActiveSalons { get; set; }
        public int TotalUsers { get; set; }
        public int TotalArtists { get; set; }
        public int TotalAppointments { get; set; }
        public int TodayAppointments { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal MonthRevenue { get; set; }
        public List<SmartSalon.Models.Salon> LatestSalons { get; set; } = new();
        public List<Appointment> LatestAppointments { get; set; } = new();

        public DashboardModel(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var token = HttpContext.Session.GetString("SuperAdminToken");
            if (token == null) return RedirectToPage("/SuperAdmin/Login");

            AdminName = HttpContext.Session.GetString("SuperAdminName") ?? "سوپرادمین";

            var today = DateTime.Today;
            var monthStart = new DateTime(today.Year, today.Month, 1);

            TotalSalons = await _db.Salons.CountAsync();
            ActiveSalons = await _db.Salons.CountAsync(s => s.IsActive);
            TotalUsers = await _db.Users.CountAsync();
            TotalArtists = await _db.Artists.CountAsync();

            TotalAppointments = await _db.Appointments.CountAsync();
            TodayAppointments = await _db.Appointments
                .CountAsync(a => a.StartTime.Date == today);

            TotalRevenue = await _db.Appointments
                .Where(a => a.Status != AppointmentStatus.Cancelled)
                .SumAsync(a => a.EstimatedPrice);

            MonthRevenue = await _db.Appointments
                .Where(a => a.Status != AppointmentStatus.Cancelled
                         && a.StartTime >= monthStart)
                .SumAsync(a => a.EstimatedPrice);

            LatestSalons = await _db.Salons
                .OrderByDescending(s => s.Id)
                .Take(5)
                .ToListAsync();

            LatestAppointments = await _db.Appointments
                .Include(a => a.Client)
                .Include(a => a.Salon)
                .OrderByDescending(a => a.Id)
                .Take(5)
                .ToListAsync();

            return Page();
        }
    }
}