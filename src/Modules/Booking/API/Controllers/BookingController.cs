using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SalonOS.Shared.Authorization;
using SalonOS.Booking.Application.DTOs;
using SalonOS.Booking.Infrastructure;
using SalonOS.Shared;

namespace SalonOS.Booking.API.Controllers;

[Route("api/bookings")]
[ApiController]
public class BookingController : ControllerBase
{
    private readonly IBookingService _bookings;
    private readonly IAuthorizationService _authz;
    private readonly ITenantContext _tenant;

    public BookingController(
        IBookingService bookings,
        IAuthorizationService authz,
        ITenantContext tenant)
    {
        _bookings = bookings;
        _authz    = authz;
        _tenant   = tenant;
    }

    [HttpGet]
    [HasPermission(Permissions.AppointmentViewAll)]
    public async Task<IActionResult> GetBookings()
    {
        var list = await _bookings.GetByTenantIdAsync(_tenant.TenantId);
        return Ok(list);
    }

    [HttpGet("{id}")]
    [HasPermission(Permissions.AppointmentViewOwn)]
    public async Task<IActionResult> GetBooking(Guid id)
    {
        var booking = await _bookings.GetByIdAsync(id, _tenant.TenantId);
        if (booking is null) return NotFound(new { message = "Booking not found" });

        var authResult = await _authz.AuthorizeAsync(User, booking, new OwnsAppointment());
        if (!authResult.Succeeded &&
            !User.HasClaim("permission", Permissions.AppointmentViewAll))
            return Forbid();

        return Ok(booking);
    }

    [HttpGet("slots")]
    [AllowAnonymous]
    public async Task<IActionResult> GetSlots(
        [FromQuery] Guid artistId,
        [FromQuery] DateTime date,
        [FromQuery] int durationMinutes = 30)
    {
        var slots = await _bookings.GetAvailableSlotsAsync(
            artistId, date, durationMinutes, _tenant.TenantId);
        return Ok(slots);
    }

    [HttpPost]
    [HasPermission(Permissions.AppointmentCreate)]
    public async Task<IActionResult> CreateBooking([FromBody] CreateBookingDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var selectionSnapshot = BuildSelectionSnapshot(dto);

        var booking = new Domain.Booking
        {
            ClientId        = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty,
            ArtistId        = dto.ArtistId,
            ServiceId       = dto.ServiceId,
            StartsAt        = dto.StartsAt,
            EndsAt          = dto.StartsAt.AddMinutes(dto.DurationMinutes),
            DurationMinutes = dto.DurationMinutes,
            EstimatedPrice  = new Money(dto.EstimatedPriceAmount, dto.Currency),
            DepositAmount   = new Money(dto.DepositAmountValue, dto.Currency),
            Notes           = dto.Notes,
            CustomerSelectionSnapshot = selectionSnapshot
        };

        var created = await _bookings.CreateAsync(booking);
        return CreatedAtAction(nameof(GetBooking), new { id = created.Id },
            new CreateBookingResponseDto
            {
                Message = "Booking created",
                Id      = created.Id,
                Deposit = dto.DepositAmountValue
            });
    }

    [HttpPut("{id}/confirm")]
    [HasPermission(Permissions.AppointmentConfirm)]
    public async Task<IActionResult> ConfirmBooking(Guid id)
    {
        var booking = await _bookings.GetByIdAsync(id, _tenant.TenantId);
        if (booking is null) return NotFound(new { message = "Booking not found" });

        if (User.HasClaim("role", "Artist"))
        {
            var authResult = await _authz.AuthorizeAsync(User, booking, new OwnsAppointment());
            if (!authResult.Succeeded) return Forbid();
        }

        await _bookings.ConfirmAsync(id, _tenant.TenantId);
        return Ok(new { message = "Booking confirmed" });
    }

    [HttpPut("{id}/complete")]
    [HasPermission(Permissions.AppointmentComplete)]
    public async Task<IActionResult> CompleteBooking(Guid id)
    {
        var booking = await _bookings.GetByIdAsync(id, _tenant.TenantId);
        if (booking is null) return NotFound(new { message = "Booking not found" });

        if (User.HasClaim("role", "Artist"))
        {
            var authResult = await _authz.AuthorizeAsync(User, booking, new OwnsAppointment());
            if (!authResult.Succeeded) return Forbid();
        }

        await _bookings.CompleteAsync(id, _tenant.TenantId);
        return Ok(new { message = "Booking completed" });
    }

    [HttpPut("{id}/cancel")]
    [HasPermission(Permissions.AppointmentCancelOwn)]
    public async Task<IActionResult> CancelBooking(Guid id)
    {
        var booking = await _bookings.GetByIdAsync(id, _tenant.TenantId);
        if (booking is null) return NotFound(new { message = "Booking not found" });

        if (!User.HasClaim("permission", Permissions.AppointmentCancelAll))
        {
            var authResult = await _authz.AuthorizeAsync(User, booking, new OwnsAppointment());
            if (!authResult.Succeeded) return Forbid();
        }

        await _bookings.CancelAsync(id, _tenant.TenantId);
        return Ok(new { message = "Booking cancelled" });
    }

    [HttpPost("{id}/rate")]
    [HasPermission(Permissions.AppointmentRate)]
    public async Task<IActionResult> RateBooking(Guid id, [FromBody] RateRequestDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var booking = await _bookings.GetByIdAsync(id, _tenant.TenantId);
        if (booking is null) return NotFound(new { message = "Booking not found" });

        var authResult = await _authz.AuthorizeAsync(User, booking, new OwnsAppointment());
        if (!authResult.Succeeded) return Forbid();

        await _bookings.RateAsync(id, dto.Rating, dto.Comment, _tenant.TenantId);
        return Ok(new { message = "Booking rated" });
    }

    private static string? BuildSelectionSnapshot(CreateBookingDto dto)
    {
        if ((dto.SelectedOptionIds == null || dto.SelectedOptionIds.Count == 0)
            && (dto.SelectedMaterialIds == null || dto.SelectedMaterialIds.Count == 0))
            return null;

        return JsonSerializer.Serialize(new
        {
            options = dto.SelectedOptionIds,
            materials = dto.SelectedMaterialIds
        });
    }
}

public class RateRequestDto
{
    [Required]
    [Range(1, 5)]
    public int Rating { get; set; }

    [MaxLength(500)]
    public string? Comment { get; set; }
}
