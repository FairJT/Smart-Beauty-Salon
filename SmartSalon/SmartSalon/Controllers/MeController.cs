using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartSalon.Data;
using SmartSalon.DTOs;
using SmartSalon.Models;
using System.Security.Claims;

namespace SmartSalon.Controllers
{
    [Route("api/me")]
    [ApiController]
    [Authorize]
    public class MeController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public MeController(ApplicationDbContext db) => _db = db;

        // ── GET /api/me/home ───────────────────────────────────────────────────
        [HttpGet("home")]
        public async Task<IActionResult> GetHome()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var user = await _db.Users.FindAsync(userId);
            if (user == null)
                return Unauthorized();

            if (user.UserType != UserType.Client)
                return Forbid();

            var now = DateTime.Now;

            var upcomingAppts = await _db.Appointments
                .Where(a => a.ClientId == userId && a.StartTime >= now
                    && (a.Status == AppointmentStatus.Pending || a.Status == AppointmentStatus.Confirmed))
                .Include(a => a.Salon)
                .Include(a => a.Artist!).ThenInclude(ar => ar.User)
                .Include(a => a.Service)
                .OrderBy(a => a.StartTime)
                .ToListAsync();

            var nextBooking = upcomingAppts
                .Select(a => new ClientNextBookingDto
                {
                    Id = a.Id,
                    StartTime = a.StartTime,
                    SalonName = a.Salon!.Name,
                    ServiceName = a.Service!.Name,
                    ArtistName = a.Artist!.User!.FirstName + " " + a.Artist!.User!.LastName,
                    Status = (int)a.Status,
                })
                .FirstOrDefault();

            var unreadCount = await _db.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .CountAsync();

            return Ok(new ClientDashboardDto
            {
                UpcomingBookings = upcomingAppts.Count,
                NextBooking = nextBooking,
                LoyaltyPoints = user.LoyaltyPoints,
                TotalVisits = user.TotalVisits,
                UnreadNotifications = unreadCount,
                FavoriteSalons = new List<FavoriteSalonDto>(),
            });
        }
    }
}
