using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SmartSalon.Data;
using SmartSalon.Models;

namespace SmartSalon.Pages.SuperAdmin
{
    public class SalonReportViewModel
    {
        public string SalonName { get; set; } = "";
        public int Total { get; set; }
        public int Confirmed { get; set; }
        public int Cancelled { get; set; }
        public decimal Revenue { get; set; }
    }

    public class DailyReportViewModel
    {
        public DateTime Date { get; set; }
        public int Count { get; set; }
        public decimal Revenue { get; set; }
    }

    public class ReportsModel : PageModel
    {
        private readonly ApplicationDbContext _db;

        public DateTime From { get; set; } = DateTime.Today.AddDays(-30);
        public DateTime To { get; set; } = DateTime.Today;
        public int TotalAppointments { get; set; }
        public int ConfirmedAppointments { get; set; }
        public int CancelledAppointments { get; set; }
        public decimal TotalRevenue { get; set; }
        public List<SalonReportViewModel> SalonReports { get; set; } = new();
        public List<DailyReportViewModel> DailyReports { get; set; } = new();

        public ReportsModel(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> OnGetAsync(DateTime? from, DateTime? to)
        {
            var token = HttpContext.Session.GetString("SuperAdminToken");
            if (token == null) return RedirectToPage("/SuperAdmin/Login");

            From = from?.Date ?? DateTime.Today.AddDays(-30);
            To = to?.Date ?? DateTime.Today;

            var appointments = await _db.Appointments
                .Include(a => a.Salon)
                .Where(a => a.StartTime.Date >= From && a.StartTime.Date <= To)
                .ToListAsync();

            TotalAppointments = appointments.Count;
            ConfirmedAppointments = appointments.Count(a => a.Status == AppointmentStatus.Confirmed);
            CancelledAppointments = appointments.Count(a => a.Status == AppointmentStatus.Cancelled);
            TotalRevenue = appointments
                .Where(a => a.Status != AppointmentStatus.Cancelled)
                .Sum(a => a.EstimatedPrice);

            // گزارش به تفکیک سالن
            SalonReports = appointments
                .GroupBy(a => a.Salon?.Name ?? "نامشخص")
                .Select(g => new SalonReportViewModel
                {
                    SalonName = g.Key,
                    Total = g.Count(),
                    Confirmed = g.Count(a => a.Status == AppointmentStatus.Confirmed),
                    Cancelled = g.Count(a => a.Status == AppointmentStatus.Cancelled),
                    Revenue = g.Where(a => a.Status != AppointmentStatus.Cancelled)
                                 .Sum(a => a.EstimatedPrice)
                })
                .OrderByDescending(r => r.Revenue)
                .ToList();

            // گزارش روزانه
            DailyReports = appointments
                .GroupBy(a => a.StartTime.Date)
                .Select(g => new DailyReportViewModel
                {
                    Date = g.Key,
                    Count = g.Count(),
                    Revenue = g.Where(a => a.Status != AppointmentStatus.Cancelled)
                               .Sum(a => a.EstimatedPrice)
                })
                .OrderByDescending(d => d.Date)
                .ToList();

            return Page();
        }
    }
}