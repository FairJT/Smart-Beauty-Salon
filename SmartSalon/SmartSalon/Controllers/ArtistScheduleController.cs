using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartSalon.Data;
using SmartSalon.DTOs;
using SmartSalon.Models;
using System.Security.Claims;

namespace SmartSalon.Controllers
{
    [Route("api/artist-schedule")]
    [ApiController]
    [Authorize]
    public class ArtistScheduleController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public ArtistScheduleController(ApplicationDbContext db) => _db = db;

        [HttpGet("my")]
        public async Task<IActionResult> GetMySchedule()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var user = await _db.Users.FindAsync(userId);
            if (user == null || user.UserType != UserType.Artist)
                return Forbid();

            var artist = await _db.Artists
                .FirstOrDefaultAsync(a => a.UserId == userId && a.IsActive);

            if (artist == null)
                return NotFound(new { message = "Artist profile not found" });

            var appointments = await _db.Appointments
                .Where(a => a.ArtistId == artist.Id)
                .Include(a => a.Client)
                .Include(a => a.Service)
                .OrderByDescending(a => a.StartTime)
                .Select(a => new AppointmentListItemDto
                {
                    Id = a.Id,
                    StartTime = a.StartTime,
                    EndTime = a.EndTime,
                    Status = (int)a.Status,
                    EstimatedPrice = a.EstimatedPrice,
                    DepositAmount = a.DepositAmount,
                    IsRated = a.IsRated,
                    Rating = a.Rating,
                    Comment = a.Comment,
                    SalonName = "",
                    ArtistName = "",
                    ServiceName = a.Service!.Name
                })
                .ToListAsync();

            return Ok(appointments);
        }

        // ── GET /api/artist-schedule/my/dashboard ──────────────────────────
        [HttpGet("my/dashboard")]
        public async Task<IActionResult> GetMyDashboard()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var user = await _db.Users.FindAsync(userId);
            if (user == null || user.UserType != UserType.Artist)
                return Forbid();

            var artist = await _db.Artists
                .FirstOrDefaultAsync(a => a.UserId == userId && a.IsActive);

            if (artist == null)
                return NotFound(new { message = "Artist profile not found" });

            var today = DateTime.Today;
            var todayEnd = today.AddDays(1);
            var monthStart = new DateTime(today.Year, today.Month, 1);
            var now = DateTime.Now;

            var todayAppts = await _db.Appointments
                .Where(a => a.ArtistId == artist.Id && a.StartTime >= today && a.StartTime < todayEnd)
                .Include(a => a.Client)
                .Include(a => a.Service)
                .ToListAsync();

            var upcoming = await _db.Appointments
                .Where(a => a.ArtistId == artist.Id && a.StartTime >= todayEnd
                    && (a.Status == AppointmentStatus.Pending || a.Status == AppointmentStatus.Confirmed))
                .CountAsync();

            var monthAppts = await _db.Appointments
                .Where(a => a.ArtistId == artist.Id && a.StartTime >= monthStart
                    && a.StartTime < todayEnd && a.Status == AppointmentStatus.Completed)
                .ToListAsync();

            var monthRevenue = monthAppts.Sum(a => (long)(a.FinalPrice ?? a.EstimatedPrice));

            var nextAppt = todayAppts
                .Where(a => a.StartTime >= now && a.Status != AppointmentStatus.Cancelled)
                .OrderBy(a => a.StartTime)
                .Select(a => new ArtistNextAppointmentDto
                {
                    Id = a.Id,
                    StartTime = a.StartTime,
                    ClientName = a.Client!.FirstName + " " + a.Client.LastName,
                    ServiceName = a.Service!.Name,
                    Status = (int)a.Status,
                })
                .FirstOrDefault();

            return Ok(new ArtistDashboardDto
            {
                TodayAppointments = todayAppts.Count,
                UpcomingAppointments = upcoming,
                NextAppointment = nextAppt,
                RatingAvg = (double)artist.RatingAvg,
                RatingCount = artist.RatingCount,
                MonthAppointments = monthAppts.Count,
                MonthRevenue = new DashboardMoney { Amount = monthRevenue },
            });
        }

        [HttpGet("my/stats")]
        public async Task<IActionResult> GetMyStats()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var user = await _db.Users.FindAsync(userId);
            if (user == null || user.UserType != UserType.Artist)
                return Forbid();

            var artist = await _db.Artists
                .FirstOrDefaultAsync(a => a.UserId == userId && a.IsActive);

            if (artist == null)
                return NotFound(new { message = "Artist profile not found" });

            var today = DateTime.Today;
            var monthStart = new DateTime(today.Year, today.Month, 1);

            var allTime = await _db.Appointments
                .Where(a => a.ArtistId == artist.Id && a.Status == AppointmentStatus.Completed)
                .ToListAsync();

            var thisMonth = allTime.Where(a => a.StartTime >= monthStart).ToList();
            var todayAppts = allTime.Where(a => a.StartTime.Date == today).ToList();

            return Ok(new
            {
                totalAppointments = allTime.Count,
                totalRevenue = allTime.Sum(a => a.EstimatedPrice),
                monthAppointments = thisMonth.Count,
                monthRevenue = thisMonth.Sum(a => a.EstimatedPrice),
                todayAppointments = todayAppts.Count,
                todayRevenue = todayAppts.Sum(a => a.EstimatedPrice),
                ratingAvg = artist.RatingAvg,
                ratingCount = artist.RatingCount
            });
        }
    }
}
