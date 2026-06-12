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
