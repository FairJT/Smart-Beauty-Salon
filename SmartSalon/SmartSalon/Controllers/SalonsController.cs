using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartSalon.Data;
using SmartSalon.Models;

namespace SmartSalon.Controllers
{
    [Route("api/salons")]
    [ApiController]
    public class SalonsController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public SalonsController(ApplicationDbContext db)
        {
            _db = db;
        }

        // ─── لیست سالن‌ها ─────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? search,
            [FromQuery] string? service,
            [FromQuery] bool? vipOnly,
            [FromQuery] int page = 1,
            [FromQuery] int size = 10)
        {
            var query = _db.Salons
                .Include(s => s.Services)
                .Where(s => s.IsActive);

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(s => s.Name.Contains(search));

            if (!string.IsNullOrWhiteSpace(service))
                query = query.Where(s => s.Services.Any(sv =>
                    sv.Name.Contains(service) && sv.IsActive));

            if (vipOnly == true)
                query = query.Where(s => s.IsVip);

            var total = await query.CountAsync();

            var data = await query
                .OrderByDescending(s => s.IsVip)
                .ThenByDescending(s => s.RatingAvg)
                .Skip((page - 1) * size)
                .Take(size)
                .Select(s => new
                {
                    s.Id,
                    s.Name,
                    s.Slug,
                    s.LogoUrl,
                    s.RatingAvg,
                    s.IsVip,
                    s.Address
                })
                .ToListAsync();

            return Ok(new { total, page, size, data });
        }

        // ─── جزئیات یک سالن ───────────────────────────────────
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var salon = await _db.Salons
                .Include(s => s.Artists)
                    .ThenInclude(a => a.User)
                .Include(s => s.Services)
                .FirstOrDefaultAsync(s => s.Id == id && s.IsActive);

            if (salon is null)
                return NotFound(new { message = "سالن یافت نشد" });

            return Ok(salon);
        }

        // ─── ساخت سالن جدید ───────────────────────────────────
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] CreateSalonDto dto)
        {
            if (await _db.Salons.AnyAsync(s => s.Slug == dto.Slug))
                return BadRequest(new { message = "این آدرس قبلاً استفاده شده است" });

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

            return Ok(new { message = "سالن با موفقیت ساخته شد", id = salon.Id });
        }

        // ─── ویرایش سالن ──────────────────────────────────────
        [HttpPut("{id:int}")]
        [Authorize]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateSalonDto dto)
        {
            var salon = await _db.Salons.FindAsync(id);

            if (salon is null)
                return NotFound(new { message = "سالن یافت نشد" });

            salon.Name = dto.Name;
            salon.Phone = dto.Phone;
            salon.Address = dto.Address;
            salon.Description = dto.Description;

            await _db.SaveChangesAsync();

            return Ok(new { message = "سالن با موفقیت ویرایش شد" });
        }
    }

    // ─── DTOs ─────────────────────────────────────────────────
    public record CreateSalonDto(
        string Name,
        string Slug,
        string? Phone,
        string? Address,
        string? Description,
        string ManagerId
    );

    public record UpdateSalonDto(
        string Name,
        string? Phone,
        string? Address,
        string? Description
    );
}