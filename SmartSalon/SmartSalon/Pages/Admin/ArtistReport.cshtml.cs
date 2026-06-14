using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SmartSalon.Data;
using SmartSalon.Models;

namespace SmartSalon.Pages.Admin
{
    public class ArtistReportViewModel
    {
        public string? ArtistName { get; set; }
        public string? PhotoUrl { get; set; }
        public decimal RatingAvg { get; set; }
        public int RatingCount { get; set; }
        public int TotalAppointments { get; set; }
        public int CompletedAppointments { get; set; }
        public int CancelledAppointments { get; set; }
        public decimal TotalRevenue { get; set; }
        public List<ServiceReportItem> ServiceReport { get; set; } = new();
        public List<DailyReportItem> DailyReport { get; set; } = new();
    }

    public class ServiceReportItem
    {
        public string ServiceName { get; set; } = "";
        public int Count { get; set; }
        public decimal Revenue { get; set; }
    }

    public class DailyReportItem
    {
        public string Date { get; set; } = "";
        public int Count { get; set; }
        public decimal Revenue { get; set; }
    }

    public class ArtistReportModel : PageModel
    {
        private readonly ApplicationDbContext _db;

        public ArtistReportViewModel? Report { get; set; }
        public string From { get; set; } = "";
        public string To { get; set; } = "";

        public ArtistReportModel(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> OnGetAsync(
            int artistId,
            DateTime? from,
            DateTime? to)
        {
            var token = HttpContext.Session.GetString("AdminToken");
            if (token == null) return RedirectToPage("/Admin/Login");

            // پیدا کردن سالن مرتبط با این مدیر
            var salon = await _db.Salons
                .FirstOrDefaultAsync(s => s.ManagerId == token);

            if (salon == null) return RedirectToPage("/Admin/Artists");

            var fromDate = from?.Date ?? DateTime.Today.AddDays(-30);
            var toDate = to?.Date ?? DateTime.Today;

            From = fromDate.ToString("yyyy-MM-dd");
            To = toDate.ToString("yyyy-MM-dd");

            // چک کنیم هنرمند متعلق به سالن این مدیر باشد
            var artist = await _db.Artists
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.Id == artistId && a.SalonId == salon.Id);

            if (artist == null) return RedirectToPage("/Admin/Artists");

            var appointments = await _db.Appointments
                .Include(a => a.Service)
                .Where(a => a.ArtistId == artistId
                         && a.StartTime.Date >= fromDate
                         && a.StartTime.Date <= toDate)
                .ToListAsync();

            Report = new ArtistReportViewModel
            {
                ArtistName = artist.User?.FirstName + " " + artist.User?.LastName,
                PhotoUrl = artist.PhotoUrl,
                RatingAvg = artist.RatingAvg,
                RatingCount = artist.RatingCount,
                TotalAppointments = appointments.Count,
                CompletedAppointments = appointments.Count(a =>
                    a.Status == AppointmentStatus.Completed),
                CancelledAppointments = appointments.Count(a =>
                    a.Status == AppointmentStatus.Cancelled),
                TotalRevenue = appointments
                    .Where(a => a.Status == AppointmentStatus.Completed)
                    .Sum(a => a.EstimatedPrice),
                ServiceReport = appointments
                    .Where(a => a.Status == AppointmentStatus.Completed)
                    .GroupBy(a => a.Service?.Name ?? "نامشخص")
                    .Select(g => new ServiceReportItem
                    {
                        ServiceName = g.Key,
                        Count = g.Count(),
                        Revenue = g.Sum(a => a.EstimatedPrice)
                    })
                    .OrderByDescending(s => s.Revenue)
                    .ToList(),
                DailyReport = appointments
                    .Where(a => a.Status == AppointmentStatus.Completed)
                    .GroupBy(a => a.StartTime.Date)
                    .Select(g => new DailyReportItem
                    {
                        Date = g.Key.ToString("yyyy-MM-dd"),
                        Count = g.Count(),
                        Revenue = g.Sum(a => a.EstimatedPrice)
                    })
                    .OrderByDescending(d => d.Date)
                    .ToList()
            };

            return Page();
        }
    }
}