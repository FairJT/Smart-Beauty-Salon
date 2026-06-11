using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartSalon.Data;
using SmartSalon.Models;

namespace SmartSalon.Controllers
{
    [Route("api/services")]
    [ApiController]
    public class ServicesController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public ServicesController(ApplicationDbContext db)
        {
            _db = db;
        }

        // ─── لیست خدمات یک سالن ───────────────────────────────
        // GET api/services?salonId=1
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int salonId)
        {
            var services = await _db.SalonServices
                .Where(s => s.SalonId == salonId && s.IsActive)
                .Select(s => new
                {
                    s.Id,
                    s.Name,
                    s.Category,
                    s.BaseDurationMinutes,
                    s.BasePrice
                })
                .ToListAsync();

            return Ok(services);
        }

        // ─── اضافه کردن خدمت جدید ─────────────────────────────
        // POST api/services
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] CreateServiceDto dto)
        {
            // بررسی وجود سالن
            var salonExists = await _db.Salons.AnyAsync(s => s.Id == dto.SalonId);
            if (!salonExists)
                return NotFound(new { message = "سالن یافت نشد" });

            var service = new SalonService
            {
                Name = dto.Name,
                Category = dto.Category,
                BaseDurationMinutes = dto.DurationMinutes,
                BasePrice = dto.Price,
                SalonId = dto.SalonId
            };

            _db.SalonServices.Add(service);
            await _db.SaveChangesAsync();

            return Ok(new { message = "خدمت با موفقیت اضافه شد", id = service.Id });
        }

        // ─── ویرایش خدمت ──────────────────────────────────────
        // PUT api/services/5
        [HttpPut("{id:int}")]
        [Authorize]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateServiceDto dto)
        {
            var service = await _db.SalonServices.FindAsync(id);

            if (service is null)
                return NotFound(new { message = "خدمت یافت نشد" });

            service.Name = dto.Name;
            service.Category = dto.Category;
            service.BaseDurationMinutes = dto.DurationMinutes;
            service.BasePrice = dto.Price;

            await _db.SaveChangesAsync();

            return Ok(new { message = "خدمت با موفقیت ویرایش شد" });
        }

        // ─── حذف خدمت ─────────────────────────────────────────
        // DELETE api/services/5
        [HttpDelete("{id:int}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            var service = await _db.SalonServices.FindAsync(id);

            if (service is null)
                return NotFound(new { message = "خدمت یافت نشد" });

            // حذف نمی‌کنیم — فقط غیرفعال می‌کنیم
            service.IsActive = false;
            await _db.SaveChangesAsync();

            return Ok(new { message = "خدمت با موفقیت حذف شد" });
        }
    }

    // ─── DTOs ─────────────────────────────────────────────────
    public record CreateServiceDto(
        string Name,
        string Category,
        int DurationMinutes,
        decimal Price,
        int SalonId
    );

    public record UpdateServiceDto(
        string Name,
        string Category,
        int DurationMinutes,
        decimal Price
    );
}