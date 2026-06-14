using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartSalon.Data;
using SmartSalon.DTOs;
using SmartSalon.Models;
using SmartSalon.Services;

namespace SmartSalon.Controllers
{
    [Route("api/admin")]
    [ApiController]
    [Authorize(Policy = "RequireSuperAdmin")]
    public class SuperAdminController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly ISalonService _salonService;

        public SuperAdminController(ApplicationDbContext db, ISalonService salonService)
        {
            _db = db;
            _salonService = salonService;
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetUsers(
            [FromQuery] string? search,
            [FromQuery] int? userType,
            [FromQuery] int page = 1,
            [FromQuery] int size = 20)
        {
            var query = _db.Users.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(u =>
                    u.PhoneNumber!.Contains(search) ||
                    u.FirstName.Contains(search) ||
                    u.LastName.Contains(search));

            if (userType.HasValue)
                query = query.Where(u => (int)u.UserType == userType.Value);

            var total = await query.CountAsync();

            var users = await query
                .OrderByDescending(u => u.CreatedAt)
                .Skip((page - 1) * size)
                .Take(size)
                .Select(u => new AdminUserDto
                {
                    Id = u.Id,
                    PhoneNumber = u.PhoneNumber ?? "",
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    UserType = u.UserType.ToString(),
                    IsActive = u.IsActive,
                    LoyaltyPoints = u.LoyaltyPoints,
                    TotalVisits = u.TotalVisits,
                    CreatedAt = u.CreatedAt
                })
                .ToListAsync();

            return Ok(new PaginatedResult<AdminUserDto>
            {
                Total = total,
                Page = page,
                Size = size,
                Data = users
            });
        }

        [HttpPut("users/{id}/type")]
        public async Task<IActionResult> ChangeUserType(string id, [FromBody] ChangeUserTypeDto dto)
        {
            var user = await _db.Users.FindAsync(id);
            if (user == null) return NotFound(new { message = "User not found" });

            if (!Enum.IsDefined(typeof(UserType), dto.UserType))
                return BadRequest(new { message = "Invalid user type" });

            user.UserType = (UserType)dto.UserType;
            await _db.SaveChangesAsync();

            return Ok(new { message = "User type updated successfully" });
        }

        [HttpPut("users/{id}/toggle-active")]
        public async Task<IActionResult> ToggleActive(string id)
        {
            var user = await _db.Users.FindAsync(id);
            if (user == null) return NotFound(new { message = "User not found" });

            user.IsActive = !user.IsActive;
            await _db.SaveChangesAsync();

            return Ok(new { message = "User status updated", isActive = user.IsActive });
        }

        [HttpGet("salons")]
        public async Task<IActionResult> GetSalons()
        {
            var salons = await _db.Salons
                .Include(s => s.Services.Where(sv => sv.IsActive))
                .Include(s => s.Artists.Where(a => a.IsActive))
                .Include(s => s.Manager)
                .OrderByDescending(s => s.IsVip)
                .ThenByDescending(s => s.RatingAvg)
                .Select(s => new AdminSalonDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    Phone = s.Phone,
                    Address = s.Address,
                    IsVip = s.IsVip,
                    IsActive = s.IsActive,
                    ManagerId = s.ManagerId,
                    ManagerName = s.Manager != null
                        ? s.Manager.FirstName + " " + s.Manager.LastName
                        : "",
                    ArtistCount = s.Artists.Count,
                    ServiceCount = s.Services.Count
                })
                .ToListAsync();

            return Ok(salons);
        }

        [HttpPost("salons")]
        public async Task<IActionResult> CreateSalon([FromBody] CreateSalonByAdminDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var manager = await _db.Users.FindAsync(dto.ManagerId);
            if (manager == null)
                return BadRequest(new { message = "Manager not found" });

            try
            {
                var salon = new Salon
                {
                    Name = dto.Name,
                    Slug = dto.Slug,
                    Phone = dto.Phone,
                    Address = dto.Address,
                    Description = dto.Description,
                    ManagerId = dto.ManagerId
                };

                _db.Salons.Add(salon);
                await _db.SaveChangesAsync();

                return Ok(new { message = "Salon created successfully", id = salon.Id });
            }
            catch (DbUpdateException)
            {
                return BadRequest(new { message = "A salon with this slug already exists" });
            }
        }

        [HttpPut("salons/{id:int}/toggle-active")]
        public async Task<IActionResult> ToggleSalonActive(int id)
        {
            var salon = await _db.Salons.FindAsync(id);
            if (salon == null) return NotFound(new { message = "Salon not found" });

            salon.IsActive = !salon.IsActive;
            await _db.SaveChangesAsync();

            return Ok(new { message = "Salon status updated", isActive = salon.IsActive });
        }

        [HttpPut("salons/{id:int}/toggle-vip")]
        public async Task<IActionResult> ToggleSalonVip(int id)
        {
            var salon = await _db.Salons.FindAsync(id);
            if (salon == null) return NotFound(new { message = "Salon not found" });

            salon.IsVip = !salon.IsVip;
            await _db.SaveChangesAsync();

            return Ok(new { message = "Salon VIP status updated", isVip = salon.IsVip });
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            var stats = new AdminStatsDto
            {
                TotalUsers = await _db.Users.CountAsync(),
                TotalSalons = await _db.Salons.CountAsync(),
                TotalAppointments = await _db.Appointments.CountAsync(),
                ActiveSalons = await _db.Salons.CountAsync(s => s.IsActive),
                TotalArtists = await _db.Artists.CountAsync(a => a.IsActive),
                TotalRevenue = await _db.Appointments
                    .Where(a => a.Status == AppointmentStatus.Completed)
                    .SumAsync(a => (double)a.EstimatedPrice)
            };

            return Ok(stats);
        }

        [HttpGet("appointments")]
        public async Task<IActionResult> GetAllAppointments(
            [FromQuery] int? salonId,
            [FromQuery] int? status,
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to,
            [FromQuery] int page = 1,
            [FromQuery] int size = 30)
        {
            var query = _db.Appointments
                .Include(a => a.Salon)
                .Include(a => a.Client)
                .Include(a => a.Artist).ThenInclude(ar => ar!.User)
                .Include(a => a.Service)
                .AsQueryable();

            if (salonId.HasValue)
                query = query.Where(a => a.SalonId == salonId.Value);

            if (status.HasValue)
                query = query.Where(a => (int)a.Status == status.Value);

            if (from.HasValue)
                query = query.Where(a => a.StartTime.Date >= from.Value.Date);

            if (to.HasValue)
                query = query.Where(a => a.StartTime.Date <= to.Value.Date);

            var total = await query.CountAsync();

            var appointments = await query
                .OrderByDescending(a => a.StartTime)
                .Skip((page - 1) * size)
                .Take(size)
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
                    SalonName = a.Salon!.Name,
                    ArtistName = a.Artist!.User!.FirstName + " " + a.Artist.User.LastName,
                    ServiceName = a.Service!.Name
                })
                .ToListAsync();

            return Ok(new PaginatedResult<AppointmentListItemDto>
            {
                Total = total,
                Page = page,
                Size = size,
                Data = appointments
            });
        }
    }
}
