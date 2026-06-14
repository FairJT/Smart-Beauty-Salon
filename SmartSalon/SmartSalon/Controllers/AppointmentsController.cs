using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartSalon.DTOs;
using SmartSalon.Services;
using System.Security.Claims;

namespace SmartSalon.Controllers
{
    [Route("api/appointments")]
    [ApiController]
    public class AppointmentsController : ControllerBase
    {
        private readonly IAppointmentService _appointmentService;

        public AppointmentsController(IAppointmentService appointmentService)
        {
            _appointmentService = appointmentService;
        }

        [HttpGet("slots")]
        public async Task<IActionResult> GetSlots(
            [FromQuery] int artistId,
            [FromQuery] DateTime date,
            [FromQuery] int duration = 30)
        {
            if (date.Date < DateTime.Today)
                return BadRequest(new { message = "Cannot book slots for past dates" });

            var slots = await _appointmentService.GetSlotsAsync(artistId, date, duration);
            if (slots == null) return NotFound(new { message = "Artist not found" });
            return Ok(slots);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] CreateAppointmentDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (dto.StartTime < DateTime.UtcNow)
                return BadRequest(new { message = "Cannot book appointments in the past" });

            var clientId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _appointmentService.CreateAsync(dto, clientId);

            if (result == null)
                return BadRequest(new { message = "This time slot is already booked" });

            return Ok(result);
        }

        [HttpGet("mine")]
        [Authorize]
        public async Task<IActionResult> GetMine()
        {
            var clientId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var list = await _appointmentService.GetMineAsync(clientId);
            return Ok(list);
        }

        [HttpPut("{id:int}/confirm")]
        [Authorize]
        public async Task<IActionResult> Confirm(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var (success, isNotFound) = await _appointmentService.ConfirmAsync(id, userId);

            if (isNotFound)
                return NotFound(new { message = "Appointment not found" });

            if (!success)
                return Forbid();

            return Ok(new { message = "Appointment confirmed" });
        }

        [HttpPut("{id:int}/complete")]
        [Authorize]
        public async Task<IActionResult> Complete(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var completed = await _appointmentService.CompleteAsync(id, userId);
            if (!completed)
                return NotFound(new { message = "Appointment not found or access denied" });

            return Ok(new { message = "Appointment completed" });
        }

        [HttpPut("{id:int}/cancel")]
        [Authorize]
        public async Task<IActionResult> CancelByClient(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var cancelled = await _appointmentService.CancelAsync(id, userId);

            if (!cancelled)
                return BadRequest(new { message = "Cannot cancel this appointment" });

            return Ok(new { message = "Appointment cancelled" });
        }

        [HttpGet("all")]
        [Authorize(Policy = "RequireSuperAdmin")]
        public async Task<IActionResult> GetAll(
            [FromQuery] int? salonId,
            [FromQuery] int? status,
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to,
            [FromQuery] int page = 1,
            [FromQuery] int size = 30)
        {
            var result = await _appointmentService.GetAllAsync(salonId, status, from, to, page, size);
            return Ok(result);
        }

        [HttpPost("{id:int}/rate")]
        [Authorize]
        public async Task<IActionResult> RateArtist(int id, [FromBody] RateRequestDto request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var (success, message, ratingAvg) = await _appointmentService.RateAsync(id, userId, request);

            if (!success)
                return BadRequest(new { message });

            return Ok(new { message, ratingAvg });
        }
    }
}
