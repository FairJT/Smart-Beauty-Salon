using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartSalon.Data;
using SmartSalon.Models;
using System.Security.Claims;

namespace SmartSalon.Controllers
{
    [Route("api/appointments")]
    [ApiController]
    public class AppointmentsController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly Services.ISmsService _sms;
        private readonly Services.INotificationService _notification;

        public AppointmentsController(
            ApplicationDbContext db,
            Services.ISmsService sms,
            Services.INotificationService notification)
        {
            _db = db;
            _sms = sms;
            _notification = notification;
        }

        // ─── تایم‌های خالی ─────────────────────────────────────
        [HttpGet("slots")]
        public async Task<IActionResult> GetSlots(
            [FromQuery] int artistId,
            [FromQuery] DateTime date,
            [FromQuery] int duration = 30)
        {
            var dayStart = date.Date.AddHours(9);
            var dayEnd = date.Date.AddHours(20);

            var booked = await _db.Appointments
                .Where(a => a.ArtistId == artistId
                         && a.StartTime.Date == date.Date
                         && a.Status != AppointmentStatus.Cancelled)
                .Select(a => new { a.StartTime, a.EndTime })
                .ToListAsync();

            var freeSlots = new List<object>();
            var current = dayStart;

            while (current.AddMinutes(duration) <= dayEnd)
            {
                var slotEnd = current.AddMinutes(duration);
                var isBusy = booked.Any(b =>
                    current < b.EndTime && slotEnd > b.StartTime);

                if (!isBusy)
                    freeSlots.Add(new
                    {
                        start = current.ToString("HH:mm"),
                        end = slotEnd.ToString("HH:mm"),
                        startFull = current
                    });

                current = current.AddMinutes(30);
            }

            return Ok(new { date = date.ToString("yyyy-MM-dd"), artistId, duration, slots = freeSlots });
        }

        // ─── ثبت رزرو ──────────────────────────────────────────
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] CreateAppointmentDto dto)
        {
            var clientId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (clientId is null) return Unauthorized();

            var endTime = dto.StartTime.AddMinutes(dto.DurationMinutes);

            var hasConflict = await _db.Appointments.AnyAsync(a =>
                a.ArtistId == dto.ArtistId &&
                a.Status != AppointmentStatus.Cancelled &&
                dto.StartTime < a.EndTime &&
                endTime > a.StartTime);

            if (hasConflict)
                return BadRequest(new { message = "این تایم قبلاً رزرو شده است" });

            var appointment = new Appointment
            {
                ClientId = clientId,
                ArtistId = dto.ArtistId,
                SalonId = dto.SalonId,
                ServiceId = dto.ServiceId,
                StartTime = dto.StartTime,
                EndTime = endTime,
                DurationMinutes = dto.DurationMinutes,
                EstimatedPrice = dto.EstimatedPrice,
                DepositAmount = dto.EstimatedPrice * 0.3m,
                Notes = dto.Notes,
                Status = AppointmentStatus.Pending
            };

            _db.Appointments.Add(appointment);
            await _db.SaveChangesAsync();

            // اطلاع به مدیر سالن
            var salon = await _db.Salons.FindAsync(dto.SalonId);
            if (salon?.ManagerId != null)
            {
                var client = await _db.Users.FindAsync(clientId);
                var dateTime = appointment.StartTime.ToString("yyyy/MM/dd HH:mm");
                await _notification.SendNewAppointmentToManagerAsync(
                    salon.ManagerId,
                    (client?.FirstName ?? "") + " " + (client?.LastName ?? ""),
                    dateTime
                );
            }

            return Ok(new
            {
                message = "رزرو با موفقیت ثبت شد",
                id = appointment.Id,
                deposit = appointment.DepositAmount
            });
        }

        // ─── رزروهای من ────────────────────────────────────────
        [HttpGet("mine")]
        [Authorize]
        public async Task<IActionResult> GetMine()
        {
            var clientId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var list = await _db.Appointments
                .Where(a => a.ClientId == clientId)
                .Include(a => a.Salon)
                .Include(a => a.Artist).ThenInclude(ar => ar!.User)
                .Include(a => a.Service)
                .OrderByDescending(a => a.StartTime)
                .Select(a => new
                {
                    a.Id,
                    a.StartTime,
                    a.EndTime,
                    a.Status,
                    a.EstimatedPrice,
                    a.DepositAmount,
                    a.IsRated,
                    a.Rating,
                    a.Comment,
                    salonName = a.Salon!.Name,
                    artistName = a.Artist!.User!.FirstName + " " + a.Artist.User.LastName,
                    serviceName = a.Service!.Name
                })
                .ToListAsync();

            return Ok(list);
        }

        // ─── تایید رزرو توسط مدیر ──────────────────────────────
        [HttpPut("{id:int}/confirm")]
        [Authorize]
        public async Task<IActionResult> Confirm(int id)
        {
            var appointment = await _db.Appointments
                .Include(a => a.Client)
                .Include(a => a.Salon)
                .Include(a => a.Artist).ThenInclude(ar => ar!.User)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (appointment is null)
                return NotFound(new { message = "رزرو یافت نشد" });

            appointment.Status = AppointmentStatus.Confirmed;
            await _db.SaveChangesAsync();

            // ارسال SMS تایید
            if (appointment.Client?.PhoneNumber != null)
            {
                var dateTime = appointment.StartTime.ToString("yyyy/MM/dd HH:mm");
                await _sms.SendAppointmentConfirmedAsync(
                    appointment.Client.PhoneNumber,
                    appointment.Client.FirstName,
                    appointment.Salon?.Name ?? "",
                    dateTime
                );
            }

            // نوتیفیکیشن داخلی
            if (appointment.ClientId != null)
            {
                var dateTime = appointment.StartTime.ToString("yyyy/MM/dd HH:mm");
                await _notification.SendAppointmentConfirmedAsync(
                    appointment.ClientId,
                    appointment.Salon?.Name ?? "",
                    dateTime
                );
            }

            return Ok(new { message = "رزرو تایید شد" });
        }

        // ─── تکمیل نوبت توسط مدیر ─────────────────────────────
        [HttpPut("{id:int}/complete")]
        [Authorize]
        public async Task<IActionResult> Complete(int id)
        {
            var appointment = await _db.Appointments
                .Include(a => a.Client)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (appointment is null)
                return NotFound(new { message = "رزرو یافت نشد" });

            appointment.Status = AppointmentStatus.Completed;

            if (appointment.Client != null)
            {
                var pointsEarned = (int)(appointment.EstimatedPrice / 10000);
                appointment.Client.LoyaltyPoints += pointsEarned;
                appointment.Client.TotalVisits += 1;
            }

            await _db.SaveChangesAsync();

            return Ok(new
            {
                message = "نوبت تکمیل شد",
                pointsEarned = (int)(appointment.EstimatedPrice / 10000)
            });
        }

        // ─── لغو نوبت توسط مشتری ───────────────────────────────
        [HttpPut("{id:int}/cancel")]
        [Authorize]
        public async Task<IActionResult> CancelByClient(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var appointment = await _db.Appointments
                .FirstOrDefaultAsync(a => a.Id == id && a.ClientId == userId);

            if (appointment == null)
                return NotFound(new { message = "نوبت یافت نشد" });

            if (appointment.Status == AppointmentStatus.Completed)
                return BadRequest(new { message = "نوبت تمام شده قابل لغو نیست" });

            if (appointment.Status == AppointmentStatus.Cancelled)
                return BadRequest(new { message = "نوبت قبلاً لغو شده" });

            if (appointment.StartTime <= DateTime.Now.AddHours(2))
                return BadRequest(new { message = "امکان لغو کمتر از ۲ ساعت قبل وجود ندارد" });

            appointment.Status = AppointmentStatus.Cancelled;
            await _db.SaveChangesAsync();

            // ارسال SMS و نوتیفیکیشن لغو
            var apt = await _db.Appointments
                .Include(a => a.Client)
                .Include(a => a.Salon)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (apt?.Client?.PhoneNumber != null)
            {
                await _sms.SendAppointmentCancelledAsync(
                    apt.Client.PhoneNumber,
                    apt.Client.FirstName,
                    apt.Salon?.Name ?? ""
                );
            }

            if (apt?.ClientId != null)
            {
                await _notification.SendAppointmentCancelledAsync(
                    apt.ClientId,
                    apt.Salon?.Name ?? ""
                );
            }

            return Ok(new { message = "نوبت با موفقیت لغو شد" });
        }

        // ─── امتیازدهی به هنرمند ───────────────────────────────
        [HttpPost("{id:int}/rate")]
        [Authorize]
        public async Task<IActionResult> RateArtist(int id, [FromBody] RateRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var appointment = await _db.Appointments
                .Include(a => a.Artist)
                .FirstOrDefaultAsync(a => a.Id == id && a.ClientId == userId);

            if (appointment == null)
                return NotFound(new { message = "نوبت یافت نشد" });

            if (appointment.Status != AppointmentStatus.Completed)
                return BadRequest(new { message = "فقط نوبت‌های تمام شده قابل امتیازدهی هستند" });

            if (appointment.IsRated)
                return BadRequest(new { message = "قبلاً امتیاز داده‌اید" });

            if (request.Rating < 1 || request.Rating > 5)
                return BadRequest(new { message = "امتیاز باید بین ۱ تا ۵ باشد" });

            appointment.IsRated = true;
            appointment.Rating = request.Rating;
            appointment.Comment = request.Comment;

            var artist = appointment.Artist!;
            var ratings = await _db.Appointments
                .Where(a => a.ArtistId == artist.Id && a.IsRated)
                .Select(a => a.Rating)
                .ToListAsync();

            ratings.Add(request.Rating);
            artist.RatingAvg = (decimal)ratings.Average();
            artist.RatingCount = ratings.Count;

            await _db.SaveChangesAsync();

            return Ok(new { message = "امتیاز با موفقیت ثبت شد", ratingAvg = artist.RatingAvg });
        }

        public class RateRequest
        {
            public int Rating { get; set; }
            public string? Comment { get; set; }
        }
    }

    // ─── DTO ───────────────────────────────────────────────────
    public record CreateAppointmentDto(
        int ArtistId,
        int SalonId,
        int ServiceId,
        DateTime StartTime,
        int DurationMinutes,
        decimal EstimatedPrice,
        string? Notes
    );
}