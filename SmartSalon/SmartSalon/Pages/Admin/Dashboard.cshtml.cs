using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SmartSalon.Data;
using SmartSalon.Models;

namespace SmartSalon.Pages.Admin
{
    public class AppointmentViewModel
    {
        public int Id { get; set; }
        public string ClientName { get; set; } = "";
        public string ServiceName { get; set; } = "";
        public string ArtistName { get; set; } = "";
        public DateTime StartTime { get; set; }
        public decimal EstimatedPrice { get; set; }
        public int Status { get; set; }
        public string StatusText => Status switch
        {
            1 => "در انتظار",
            2 => "تایید شده",
            3 => "در حال انجام",
            4 => "تمام شده",
            5 => "لغو شده",
            _ => "نامشخص"
        };
        public string StatusBadge => Status switch
        {
            1 => "bg-warning text-dark",
            2 => "bg-success",
            3 => "bg-primary",
            4 => "bg-secondary",
            5 => "bg-danger",
            _ => "bg-secondary"
        };
    }

    public class DashboardModel : PageModel
    {
        private readonly ApplicationDbContext _db;

        public string AdminName { get; set; } = "";
        public int TodayCount { get; set; }
        public int PendingCount { get; set; }
        public int ConfirmedCount { get; set; }
        public decimal TodayRevenue { get; set; }
        public List<AppointmentViewModel> TodayAppointments { get; set; } = new();

        public DashboardModel(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var token = HttpContext.Session.GetString("AdminToken");
            if (string.IsNullOrEmpty(token))
                return RedirectToPage("/Admin/Login");

            AdminName = HttpContext.Session.GetString("AdminName") ?? "مدیر";

            // پیدا کردن سالن مرتبط با این مدیر
            var salon = await _db.Salons
                .FirstOrDefaultAsync(s => s.ManagerId == token);

            if (salon == null)
            {
                // اگر سالنی ندارد پیام نشان دهد
                TodayAppointments = new();
                return Page();
            }

            var today = DateTime.Today;

            // فقط نوبت‌های همین سالن
            var todayApps = await _db.Appointments
                .Include(a => a.Client)
                .Include(a => a.Artist).ThenInclude(ar => ar!.User)
                .Include(a => a.Service)
                .Where(a => a.StartTime.Date == today && a.SalonId == salon.Id)
                .ToListAsync();

            TodayCount = todayApps.Count;
            PendingCount = todayApps.Count(a => a.Status == AppointmentStatus.Pending);
            ConfirmedCount = todayApps.Count(a => a.Status == AppointmentStatus.Confirmed);
            TodayRevenue = todayApps
                .Where(a => a.Status != AppointmentStatus.Cancelled)
                .Sum(a => a.EstimatedPrice);

            TodayAppointments = todayApps.Select(a => new AppointmentViewModel
            {
                Id = a.Id,
                ClientName = a.Client?.FirstName + " " + a.Client?.LastName,
                ServiceName = a.Service?.Name ?? "",
                ArtistName = a.Artist?.User?.FirstName + " " + a.Artist?.User?.LastName,
                StartTime = a.StartTime,
                EstimatedPrice = a.EstimatedPrice,
                Status = (int)a.Status,
            }).OrderBy(a => a.StartTime).ToList();

            return Page();
        }

        public async Task<IActionResult> OnPostConfirmAsync(int id)
        {
            var token = HttpContext.Session.GetString("AdminToken");
            if (string.IsNullOrEmpty(token))
                return RedirectToPage("/Admin/Login");

            var a = await _db.Appointments
                .Include(a => a.Salon)
                .FirstOrDefaultAsync(a => a.Id == id && a.Salon!.ManagerId == token);

            if (a != null)
            {
                a.Status = AppointmentStatus.Confirmed;
                await _db.SaveChangesAsync();
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostCancelAsync(int id)
        {
            var token = HttpContext.Session.GetString("AdminToken");
            if (string.IsNullOrEmpty(token))
                return RedirectToPage("/Admin/Login");

            var a = await _db.Appointments
                .Include(a => a.Salon)
                .FirstOrDefaultAsync(a => a.Id == id && a.Salon!.ManagerId == token);

            if (a != null)
            {
                a.Status = AppointmentStatus.Cancelled;
                await _db.SaveChangesAsync();
            }
            return RedirectToPage();
        }
    }
}