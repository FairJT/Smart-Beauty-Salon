using FluentValidation;
using SalonOS.Booking.Application.DTOs;

namespace SalonOS.Booking.Application.Validators;

/// <summary>
/// Validator for CreateBookingDto.
/// </summary>
public class CreateBookingValidator : AbstractValidator<CreateBookingDto>
{
    public CreateBookingValidator()
    {
        RuleFor(x => x.ArtistId)
            .NotEmpty().WithMessage("Artist ID is required");

        RuleFor(x => x.ServiceId)
            .NotEmpty().WithMessage("Service ID is required");

        RuleFor(x => x.StartsAt)
            .NotEmpty().WithMessage("Start time is required")
            .GreaterThan(DateTime.UtcNow).WithMessage("Start time must be in the future");

        RuleFor(x => x.DurationMinutes)
            .InclusiveBetween(5, 480).WithMessage("Duration must be between 5 and 480 minutes");

        RuleFor(x => x.Notes)
            .MaximumLength(500).WithMessage("Notes must not exceed 500 characters");

        RuleFor(x => x.CustomerSelectionSnapshot)
            .MaximumLength(1000).WithMessage("Customer selection snapshot must not exceed 1000 characters");
    }
}
