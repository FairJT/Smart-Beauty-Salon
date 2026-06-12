using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SalonOS.Api.Authorization;
using SalonOS.Booking.Application.DTOs;
using SalonOS.Booking.Infrastructure;
using SalonOS.Shared;
using SalonOS.Shared.Authorization;

namespace SalonOS.Booking.API.Controllers;

/// <summary>
/// Booking controller — all actions protected by permission claims (R2).
/// "own"-scoped actions additionally call OwnsAppointment to prevent IDOR (Task 5.2).
/// Tenant id comes from ITenantContext, never from request input (R3).
/// </summary>
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

    // ── GET /api/bookings ─────────────────────────────────────────────────────
    // SalonManager / Receptionist see all; Artist / Client see own (filtered in service).
    [HttpGet]
    [HasPermission(Permissions.AppointmentViewAll)]
    public async Task<IActionResult> GetBookings()
    {
        var list = await _bookings.GetByTenantIdAsync(_tenant.TenantId);
        return Ok(list);
    }

    // ── GET /api/bookings/{id} ────────────────────────────────────────────────
    [HttpGet("{id}")]
    [HasPermission(Permissions.AppointmentViewOwn)]   // minimum; .all roles also hold this
    public async Task<IActionResult> GetBooking(Guid id)
    {
        var booking = await _bookings.GetByIdAsync(id, _tenant.TenantId);
        if (booking is null) return NotFound(new { message = "Booking not found" });

        // Ownership check for own-scoped callers
        var authResult = await _authz.AuthorizeAsync(User, booking, new OwnsAppointment());
        if (!authResult.Succeeded &&
            !User.HasClaim("permission", Permissions.AppointmentViewAll))
            return Forbid();

        return Ok(booking);
    }

    // ── GET /api/bookings/slots ───────────────────────────────────────────────
    [HttpGet("slots")]
    [AllowAnonymous]
    public IActionResult GetSlots([FromQuery] Guid artistId, [FromQuery] DateTime date)
    {
        // Public endpoint — available slot lookup requires no auth
        return Ok(new List<SlotDto>());
    }

    // ── POST /api/bookings ────────────────────────────────────────────────────
    [HttpPost]
    [HasPermission(Permissions.AppointmentCreate)]
    public async Task<IActionResult> CreateBooking([FromBody] CreateBookingDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        // TenantId is stamped by AppDbContext.SaveChanges — never from dto (R4)
        var booking = new SalonOS.Booking.Domain.Booking
        {
            ClientId        = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty,
            ArtistId        = dto.ArtistId,
            ServiceId       = dto.ServiceId,
            StartsAt        = dto.StartsAt,
            EndsAt          = dto.StartsAt.AddMinutes(dto.DurationMinutes),
            DurationMinutes = dto.DurationMinutes,
            EstimatedPrice  = new SalonOS.Shared.Money(dto.EstimatedPrice, dto.Currency ?? "IRR"),
            DepositAmount   = new SalonOS.Shared.Money(dto.DepositAmount, dto.Currency ?? "IRR"),
        };

        var created = await _bookings.CreateAsync(booking);
        return CreatedAtAction(nameof(GetBooking), new { id = created.Id },
            new CreateBookingResponseDto
            {
                Message = "Booking created",
                Id      = created.Id,
                Deposit = dto.DepositAmount
            });
    }

    // ── PUT /api/bookings/{id}/confirm ────────────────────────────────────────
    // SalonManager / Receptionist use .all; Artist uses own-confirm.
    [HttpPut("{id}/confirm")]
    [HasPermission(Permissions.AppointmentConfirm)]
    public async Task<IActionResult> ConfirmBooking(Guid id)
    {
        var booking = await _bookings.GetByIdAsync(id, _tenant.TenantId);
        if (booking is null) return NotFound(new { message = "Booking not found" });

        // Artists can only confirm their own bookings
        if (User.HasClaim("role", "Artist"))
        {
            var authResult = await _authz.AuthorizeAsync(User, booking, new OwnsAppointment());
            if (!authResult.Succeeded) return Forbid();
        }

        await _bookings.ConfirmAsync(id, _tenant.TenantId);
        return Ok(new { message = "Booking confirmed" });
    }

    // ── PUT /api/bookings/{id}/complete ───────────────────────────────────────
    [HttpPut("{id}/complete")]
    [HasPermission(Permissions.AppointmentComplete)]
    public async Task<IActionResult> CompleteBooking(Guid id)
    {
        var booking = await _bookings.GetByIdAsync(id, _tenant.TenantId);
        if (booking is null) return NotFound(new { message = "Booking not found" });

        // Artists can only complete their own bookings
        if (User.HasClaim("role", "Artist"))
        {
            var authResult = await _authz.AuthorizeAsync(User, booking, new OwnsAppointment());
            if (!authResult.Succeeded) return Forbid();
        }

        await _bookings.CompleteAsync(id, _tenant.TenantId);
        return Ok(new { message = "Booking completed" });
    }

    // ── PUT /api/bookings/{id}/cancel ─────────────────────────────────────────
    // SalonManager / Receptionist hold cancel.all; Artist / Client hold cancel.own.
    [HttpPut("{id}/cancel")]
    [HasPermission(Permissions.AppointmentCancelOwn)]  // minimum permission gate
    public async Task<IActionResult> CancelBooking(Guid id)
    {
        var booking = await _bookings.GetByIdAsync(id, _tenant.TenantId);
        if (booking is null) return NotFound(new { message = "Booking not found" });

        // If the caller only holds cancel.own (Artist or Client), enforce ownership
        if (!User.HasClaim("permission", Permissions.AppointmentCancelAll))
        {
            var authResult = await _authz.AuthorizeAsync(User, booking, new OwnsAppointment());
            if (!authResult.Succeeded) return Forbid();
        }

        await _bookings.CancelAsync(id, _tenant.TenantId);
        return Ok(new { message = "Booking cancelled" });
    }

    // ── POST /api/bookings/{id}/rate ──────────────────────────────────────────
    [HttpPost("{id}/rate")]
    [HasPermission(Permissions.AppointmentRate)]
    public async Task<IActionResult> RateBooking(Guid id, [FromBody] RateRequestDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var booking = await _bookings.GetByIdAsync(id, _tenant.TenantId);
        if (booking is null) return NotFound(new { message = "Booking not found" });

        // Client can only rate their own booking
        var authResult = await _authz.AuthorizeAsync(User, booking, new OwnsAppointment());
        if (!authResult.Succeeded) return Forbid();

        await _bookings.RateAsync(id, dto.Rating, dto.Comment, _tenant.TenantId);
        return Ok(new { message = "Booking rated" });
    }
}

/// <summary>DTO for rating a booking.</summary>
public class RateRequestDto
{
    [Required]
    [Range(1, 5)]
    public int Rating { get; set; }

    [MaxLength(500)]
    public string? Comment { get; set; }
}
