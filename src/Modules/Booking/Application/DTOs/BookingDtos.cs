using System.ComponentModel.DataAnnotations;

namespace SalonOS.Booking.Application.DTOs;

public class CreateBookingDto
{
    [Required]
    public Guid ArtistId { get; set; }

    [Required]
    public Guid ServiceId { get; set; }

    [Required]
    public DateTime StartsAt { get; set; }

    [Required]
    [Range(5, 480)]
    public int DurationMinutes { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }

    [MaxLength(1000)]
    public string? CustomerSelectionSnapshot { get; set; }

    [Required]
    public long EstimatedPriceAmount { get; set; }

    public string Currency { get; set; } = "IRR";

    [Required]
    public long DepositAmountValue { get; set; }

    public List<Guid>? SelectedOptionIds { get; set; }
    public List<Guid>? SelectedMaterialIds { get; set; }
}

public class BookingDto
{
    public Guid Id { get; set; }
    public string ClientId { get; set; } = string.Empty;
    public Guid ArtistId { get; set; }
    public string? ArtistName { get; set; }
    public Guid ServiceId { get; set; }
    public string? ServiceName { get; set; }
    public DateTime StartsAt { get; set; }
    public DateTime EndsAt { get; set; }
    public int DurationMinutes { get; set; }
    public long EstimatedPriceAmount { get; set; }
    public string EstimatedPriceCurrency { get; set; } = string.Empty;
    public long? FinalPriceAmount { get; set; }
    public string? FinalPriceCurrency { get; set; }
    public long DepositAmountValue { get; set; }
    public string DepositCurrency { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public bool IsRated { get; set; }
    public int? Rating { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class SlotDto
{
    public DateTime StartsAt { get; set; }
    public DateTime EndsAt { get; set; }
    public bool IsAvailable { get; set; }
}

public class CreateBookingResponseDto
{
    public string Message { get; set; } = string.Empty;
    public Guid Id { get; set; }
    public long Deposit { get; set; }
}
