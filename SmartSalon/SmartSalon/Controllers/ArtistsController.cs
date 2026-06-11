using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartSalon.DTOs;
using SmartSalon.Services;
using System.Security.Claims;

namespace SmartSalon.Controllers
{
    [Route("api/artists")]
    [ApiController]
    public class ArtistsController : ControllerBase
    {
        private readonly IArtistService _artistService;
        private readonly ISalonService _salonService;

        public ArtistsController(IArtistService artistService, ISalonService salonService)
        {
            _artistService = artistService;
            _salonService = salonService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int salonId)
        {
            var artists = await _artistService.GetArtistsBySalonAsync(salonId);
            return Ok(artists);
        }

        [HttpGet("{id:int}/report")]
        [Authorize]
        public async Task<IActionResult> GetReport(
            int id,
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to,
            [FromQuery] int page = 1,
            [FromQuery] int size = 30)
        {
            var report = await _artistService.GetReportAsync(id, from, to, page, size);
            if (report == null) return NotFound(new { message = "Artist not found" });
            return Ok(report);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] CreateArtistDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var isManager = await _salonService.IsSalonManagerAsync(dto.SalonId, userId);
            if (!isManager) return Forbid();

            var id = await _artistService.CreateArtistAsync(dto);
            if (id == null)
                return BadRequest(new { message = "Salon or user not found, or artist already exists" });

            return Ok(new { message = "Artist added successfully", id });
        }

        [HttpPut("{id:int}")]
        [Authorize]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateArtistDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var artistSalonId = await _artistService.GetSalonIdAsync(id);
            if (artistSalonId == null) return NotFound(new { message = "Artist not found" });

            var isManager = await _salonService.IsSalonManagerAsync(artistSalonId.Value, userId);
            if (!isManager) return Forbid();

            var updated = await _artistService.UpdateArtistAsync(id, dto);
            if (!updated) return NotFound(new { message = "Artist not found" });

            return Ok(new { message = "Artist updated successfully" });
        }

        [HttpDelete("{id:int}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var artistSalonId = await _artistService.GetSalonIdAsync(id);
            if (artistSalonId == null) return NotFound(new { message = "Artist not found" });

            var isManager = await _salonService.IsSalonManagerAsync(artistSalonId.Value, userId);
            if (!isManager) return Forbid();

            var (success, message) = await _artistService.DeleteArtistAsync(id);
            if (!success) return BadRequest(new { message });

            return Ok(new { message = "Artist deleted successfully" });
        }

        [HttpPost("{id:int}/photo")]
        [Authorize]
        public async Task<IActionResult> UploadPhoto(int id, IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "No file selected" });

            var allowedTypes = new[] { "image/jpeg", "image/png", "image/webp" };
            if (!allowedTypes.Contains(file.ContentType))
                return BadRequest(new { message = "Only JPG, PNG and WebP files are allowed" });

            if (file.Length > 5 * 1024 * 1024)
                return BadRequest(new { message = "File size must not exceed 5MB" });

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var artistSalonId = await _artistService.GetSalonIdAsync(id);
            if (artistSalonId == null) return NotFound(new { message = "Artist not found" });

            var isManager = await _salonService.IsSalonManagerAsync(artistSalonId.Value, userId);
            if (!isManager) return Forbid();

            var uploadsFolder = Path.Combine(
                Directory.GetCurrentDirectory(), "wwwroot", "uploads", "artists");
            Directory.CreateDirectory(uploadsFolder);

            var fileName = $"artist_{id}_{Guid.NewGuid():N}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
                await file.CopyToAsync(stream);

            var photoUrl = $"/uploads/artists/{fileName}";
            var saved = await _artistService.UploadPhotoAsync(id, photoUrl);
            if (!saved) return NotFound(new { message = "Artist not found" });

            return Ok(new { message = "Photo uploaded successfully", photoUrl });
        }
    }
}
