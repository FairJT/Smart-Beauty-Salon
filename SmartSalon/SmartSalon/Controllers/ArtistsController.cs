using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartSalon.Data;
using SmartSalon.Models;

namespace SmartSalon.Controllers
{
    [Route("api/artists")]
    [ApiController]
    public class ArtistsController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public ArtistsController(ApplicationDbContext db)
        {
            _db = db;
        }

        // ─── لیست هنرمندان یک سالن ────────────────────────────
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int salonId)
        {
            var artists = await _db.Artists
                .Where(a => a.SalonId == salonId && a.IsActive)
                .Include(a => a.User)
                .Select(a => new
                {
                    a.Id,
                    a.BioShort,
                    a.RatingAvg,
                    a.RatingCount,
                    a.ContractType,
                    a.PhotoUrl,
                    firstName = a.User!.FirstName,
                    lastName = a.User.LastName,
                    avatar = a.User.AvatarUrl
                })
                .ToListAsync();

            return Ok(artists);
        }

        // ─── گزارش درآمد هنرمند ───────────────────────────────
        [HttpGet("{id:int}/report")]
        [Authorize]
        public async Task<IActionResult> GetReport(
            int id,
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to)
        {
            var artist = await _db.Artists
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (artist == null)
                return NotFound(new { message = "هنرمند یافت نشد" });

            var fromDate = from?.Date ?? DateTime.Today.AddDays(-30);
            var toDate = to?.Date ?? DateTime.Today;

            var appointments = await _db.Appointments
                .Include(a => a.Service)
                .Where(a => a.ArtistId == id
                         && a.StartTime.Date >= fromDate
                         && a.StartTime.Date <= toDate)
                .ToListAsync();

            var totalAppointments = appointments.Count;
            var completedAppointments = appointments.Count(a =>
                a.Status == AppointmentStatus.Completed);
            var cancelledAppointments = appointments.Count(a =>
                a.Status == AppointmentStatus.Cancelled);
            var totalRevenue = appointments
                .Where(a => a.Status == AppointmentStatus.Completed)
                .Sum(a => a.EstimatedPrice);
            var avgRating = appointments
                .Where(a => a.IsRated)
                .Select(a => (double)a.Rating)
                .DefaultIfEmpty(0)
                .Average();

            // گزارش روزانه
            var dailyReport = appointments
                .Where(a => a.Status == AppointmentStatus.Completed)
                .GroupBy(a => a.StartTime.Date)
                .Select(g => new
                {
                    date = g.Key.ToString("yyyy-MM-dd"),
                    count = g.Count(),
                    revenue = g.Sum(a => a.EstimatedPrice)
                })
                .OrderBy(d => d.date)
                .ToList();

            // گزارش بر اساس خدمت
            var serviceReport = appointments
                .Where(a => a.Status == AppointmentStatus.Completed)
                .GroupBy(a => a.Service?.Name ?? "نامشخص")
                .Select(g => new
                {
                    serviceName = g.Key,
                    count = g.Count(),
                    revenue = g.Sum(a => a.EstimatedPrice)
                })
                .OrderByDescending(s => s.revenue)
                .ToList();

            return Ok(new
            {
                artistName = artist.User?.FirstName + " " + artist.User?.LastName,
                photoUrl = artist.PhotoUrl,
                ratingAvg = artist.RatingAvg,
                ratingCount = artist.RatingCount,
                fromDate = fromDate.ToString("yyyy-MM-dd"),
                toDate = toDate.ToString("yyyy-MM-dd"),
                totalAppointments,
                completedAppointments,
                cancelledAppointments,
                totalRevenue,
                avgRating,
                dailyReport,
                serviceReport
            });
        }

        // ─── اضافه کردن هنرمند به سالن ────────────────────────
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] CreateArtistDto dto)
        {
            var salonExists = await _db.Salons.AnyAsync(s => s.Id == dto.SalonId);
            if (!salonExists)
                return NotFound(new { message = "سالن یافت نشد" });

            var userExists = await _db.Users.AnyAsync(u => u.Id == dto.UserId);
            if (!userExists)
                return NotFound(new { message = "کاربر یافت نشد" });

            var alreadyExists = await _db.Artists
                .AnyAsync(a => a.UserId == dto.UserId && a.SalonId == dto.SalonId);
            if (alreadyExists)
                return BadRequest(new { message = "این کاربر قبلاً به این سالن اضافه شده" });

            var artist = new Artist
            {
                UserId = dto.UserId,
                SalonId = dto.SalonId,
                BioShort = dto.BioShort,
                BioLong = dto.BioLong,
                ContractType = dto.ContractType
            };

            _db.Artists.Add(artist);
            await _db.SaveChangesAsync();

            return Ok(new { message = "هنرمند با موفقیت اضافه شد", id = artist.Id });
        }

        // ─── ویرایش هنرمند ────────────────────────────────────
        [HttpPut("{id:int}")]
        [Authorize]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateArtistDto dto)
        {
            var artist = await _db.Artists.FindAsync(id);
            if (artist is null)
                return NotFound(new { message = "هنرمند یافت نشد" });

            artist.BioShort = dto.BioShort;
            artist.BioLong = dto.BioLong;
            artist.ContractType = dto.ContractType;

            await _db.SaveChangesAsync();

            return Ok(new { message = "هنرمند با موفقیت ویرایش شد" });
        }

        // ─── حذف هنرمند از سالن ───────────────────────────────
        [HttpDelete("{id:int}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            var artist = await _db.Artists.FindAsync(id);
            if (artist is null)
                return NotFound(new { message = "هنرمند یافت نشد" });

            artist.IsActive = false;
            await _db.SaveChangesAsync();

            return Ok(new { message = "هنرمند با موفقیت حذف شد" });
        }

        // ─── آپلود عکس پروفایل هنرمند ─────────────────────────
        [HttpPost("{id:int}/photo")]
        public async Task<IActionResult> UploadPhoto(int id, IFormFile file)
        {
            var artist = await _db.Artists.FindAsync(id);
            if (artist == null)
                return NotFound(new { message = "هنرمند یافت نشد" });

            if (file == null || file.Length == 0)
                return BadRequest(new { message = "فایلی انتخاب نشده" });

            var allowedTypes = new[] { "image/jpeg", "image/png", "image/webp" };
            if (!allowedTypes.Contains(file.ContentType))
                return BadRequest(new { message = "فقط فایل‌های JPG، PNG و WebP مجاز هستند" });

            if (file.Length > 5 * 1024 * 1024)
                return BadRequest(new { message = "حجم فایل نباید بیشتر از ۵ مگابایت باشد" });

            var uploadsFolder = Path.Combine(
                Directory.GetCurrentDirectory(), "wwwroot", "uploads", "artists");
            Directory.CreateDirectory(uploadsFolder);

            if (!string.IsNullOrEmpty(artist.PhotoUrl))
            {
                var oldPath = Path.Combine(
                    Directory.GetCurrentDirectory(), "wwwroot",
                    artist.PhotoUrl.TrimStart('/'));
                if (System.IO.File.Exists(oldPath))
                    System.IO.File.Delete(oldPath);
            }

            var fileName = $"artist_{id}_{Guid.NewGuid():N}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
                await file.CopyToAsync(stream);

            artist.PhotoUrl = $"/uploads/artists/{fileName}";
            await _db.SaveChangesAsync();

            return Ok(new { message = "عکس با موفقیت آپلود شد", photoUrl = artist.PhotoUrl });
        }
    }

    // ─── DTOs ─────────────────────────────────────────────────
    public record CreateArtistDto(
        string UserId,
        int SalonId,
        string BioShort,
        string? BioLong,
        ContractType ContractType
    );

    public record UpdateArtistDto(
        string BioShort,
        string? BioLong,
        ContractType ContractType
    );
}