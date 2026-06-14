using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartSalon.Data;
using SmartSalon.DTOs;
using SmartSalon.Models;
using SmartSalon.Services;
using System.Security.Claims;

namespace SmartSalon.Controllers
{
    [Route("api/salons")]
    [ApiController]
    public class SalonsController : ControllerBase
    {
        private readonly ISalonService _salonService;
        private readonly ApplicationDbContext _db;

        public SalonsController(ISalonService salonService, ApplicationDbContext db)
        {
            _salonService = salonService;
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? search,
            [FromQuery] string? service,
            [FromQuery] bool? vipOnly,
            [FromQuery] int page = 1,
            [FromQuery] int size = 10)
        {
            var result = await _salonService.GetSalonsAsync(search, service, vipOnly, page, size);
            return Ok(result);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var salon = await _salonService.GetSalonByIdAsync(id);
            if (salon == null) return NotFound(new { message = "Salon not found" });
            return Ok(salon);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] CreateSalonDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var id = await _salonService.CreateSalonAsync(dto);
                return Ok(new { message = "Salon created successfully", id });
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException)
            {
                return BadRequest(new { message = "A salon with this slug already exists" });
            }
        }

        [HttpPut("{id:int}")]
        [Authorize]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateSalonDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var salon = await _salonService.GetSalonByIdAsync(id);
            if (salon == null) return NotFound(new { message = "Salon not found" });

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var isManager = await _salonService.IsSalonManagerAsync(id, userId);
            if (!isManager)
                return Forbid();

            var updated = await _salonService.UpdateSalonAsync(id, dto, userId);
            return Ok(new { message = "Salon updated successfully" });
        }

        // ── GET /api/salons/my-dashboard ─────────────────────────────────────
        [HttpGet("my-dashboard")]
        [Authorize]
        public async Task<IActionResult> GetMyDashboard()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var salon = await _db.Salons
                .FirstOrDefaultAsync(s => s.ManagerId == userId && s.IsActive);

            if (salon == null)
                return NotFound(new { message = "No salon found for this manager" });

            var result = await BuildDashboardAsync(salon.Id, userId, null, null);
            return Ok(result);
        }

        // ── GET /api/salons/{id}/dashboard ─────────────────────────────────────
        [HttpGet("{id:int}/dashboard")]
        [Authorize]
        public async Task<IActionResult> GetDashboard(int id,
            [FromQuery] DateTime? from, [FromQuery] DateTime? to)
        {
            var salon = await _db.Salons.FindAsync(id);
            if (salon == null)
                return NotFound(new { message = "Salon not found" });

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var isManager = await _salonService.IsSalonManagerAsync(id, userId);
            if (!isManager)
                return Forbid();

            var result = await BuildDashboardAsync(id, userId, from, to);
            return Ok(result);
        }

        private async Task<SalonManagerDashboardDto> BuildDashboardAsync(int salonId, string userId, DateTime? from, DateTime? to)
        {
            var todayStart = DateTime.Today;
            var todayEnd = todayStart.AddDays(1);
            var rangeFrom = from ?? todayStart;
            var rangeTo = to ?? todayEnd;

            var todayAppts = await _db.Appointments
                .Where(a => a.SalonId == salonId && a.StartTime >= todayStart && a.StartTime < todayEnd)
                .ToListAsync();

            var upcoming = await _db.Appointments
                .Where(a => a.SalonId == salonId && a.StartTime >= todayEnd
                    && (a.Status == AppointmentStatus.Pending || a.Status == AppointmentStatus.Confirmed))
                .CountAsync();

            var revenue = await _db.Appointments
                .Where(a => a.SalonId == salonId && a.Status == AppointmentStatus.Completed
                    && a.StartTime >= rangeFrom && a.StartTime < rangeTo)
                .SumAsync(a => (long)(a.FinalPrice ?? a.EstimatedPrice));

            var artists = await _db.Artists
                .Where(a => a.SalonId == salonId && a.IsActive)
                .Include(a => a.User)
                .ToListAsync();

            var artistUtil = artists.Select(a =>
            {
                var todayCount = todayAppts.Count(t => t.ArtistId == a.Id);
                var completedToday = todayAppts.Count(t => t.ArtistId == a.Id
                    && t.Status == AppointmentStatus.Completed);
                return new ArtistUtilizationDto
                {
                    ArtistId = a.Id,
                    ArtistName = $"{a.User?.FirstName} {a.User?.LastName}",
                    TodayAppointments = todayCount,
                    CompletedToday = completedToday,
                    UtilizationPercent = 8 > 0 ? (completedToday / 8.0) * 100 : 0,
                };
            }).ToList();

            var activeServices = await _db.SalonServices
                .CountAsync(s => s.SalonId == salonId && s.IsActive);

            var subscription = await _db.SalonPackageSubscriptions
                .Where(s => s.SalonId == salonId)
                .OrderByDescending(s => s.StartDate)
                .FirstOrDefaultAsync();

            return new SalonManagerDashboardDto
            {
                TodayAppointments = todayAppts.Count,
                UpcomingAppointments = upcoming,
                Revenue = new DashboardMoney { Amount = revenue },
                ArtistUtilization = artistUtil,
                ActiveServiceCount = activeServices,
                ActiveArtistCount = artists.Count,
                SubscriptionStatus = subscription != null && subscription.IsActive ? "active" : "none",
            };
        }

        [HttpDelete("{id:int}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            var salon = await _salonService.GetSalonByIdAsync(id);
            if (salon == null) return NotFound(new { message = "Salon not found" });

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var isManager = await _salonService.IsSalonManagerAsync(id, userId);
            if (!isManager) return Forbid();

            await _salonService.DeleteSalonAsync(id, userId);
            return Ok(new { message = "Salon deleted successfully" });
        }
    }
}
