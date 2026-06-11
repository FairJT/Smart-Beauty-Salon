using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartSalon.DTOs;
using SmartSalon.Services;
using System.Security.Claims;

namespace SmartSalon.Controllers
{
    [Route("api/salons")]
    [ApiController]
    public class SalonsController : ControllerBase
    {
        private readonly ISalonService _salonService;

        public SalonsController(ISalonService salonService) => _salonService = salonService;

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

            var id = await _salonService.CreateSalonAsync(dto);
            return Ok(new { message = "Salon created successfully", id });
        }

        [HttpPut("{id:int}")]
        [Authorize]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateSalonDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var isManager = await _salonService.IsSalonManagerAsync(id, userId);
            if (!isManager)
                return Forbid();

            var updated = await _salonService.UpdateSalonAsync(id, dto, userId);
            if (!updated) return NotFound(new { message = "Salon not found" });

            return Ok(new { message = "Salon updated successfully" });
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
