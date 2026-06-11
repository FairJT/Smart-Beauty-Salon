using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartSalon.DTOs;
using SmartSalon.Services;
using System.Security.Claims;

namespace SmartSalon.Controllers
{
    [Route("api/services")]
    [ApiController]
    public class ServicesController : ControllerBase
    {
        private readonly IServiceService _serviceService;
        private readonly ISalonService _salonService;

        public ServicesController(IServiceService serviceService, ISalonService salonService)
        {
            _serviceService = serviceService;
            _salonService = salonService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int salonId)
        {
            var services = await _serviceService.GetServicesBySalonAsync(salonId);
            return Ok(services);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var service = await _serviceService.GetByIdAsync(id);
            if (service == null) return NotFound(new { message = "Service not found" });
            return Ok(service);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] CreateServiceDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var isManager = await _salonService.IsSalonManagerAsync(dto.SalonId, userId);
            if (!isManager) return Forbid();

            var id = await _serviceService.CreateServiceAsync(dto);
            if (id == null) return NotFound(new { message = "Salon not found" });

            return Ok(new { message = "Service added successfully", id });
        }

        [HttpPut("{id:int}")]
        [Authorize]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateServiceDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var serviceSalonId = await _serviceService.GetSalonIdAsync(id);
            if (serviceSalonId == null) return NotFound(new { message = "Service not found" });

            var isManager = await _salonService.IsSalonManagerAsync(serviceSalonId.Value, userId);
            if (!isManager) return Forbid();

            var updated = await _serviceService.UpdateServiceAsync(id, dto);
            if (!updated) return NotFound(new { message = "Service not found" });

            return Ok(new { message = "Service updated successfully" });
        }

        [HttpDelete("{id:int}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var serviceSalonId = await _serviceService.GetSalonIdAsync(id);
            if (serviceSalonId == null) return NotFound(new { message = "Service not found" });

            var isManager = await _salonService.IsSalonManagerAsync(serviceSalonId.Value, userId);
            if (!isManager) return Forbid();

            var (success, message) = await _serviceService.DeleteServiceAsync(id);
            if (!success) return BadRequest(new { message });

            return Ok(new { message = "Service deleted successfully" });
        }
    }
}
