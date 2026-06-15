using FluentValidation;
using SalonOS.Catalog.Application.DTOs;

namespace SalonOS.Catalog.Application.Validators;

public class CreateServiceOptionValidator : AbstractValidator<CreateServiceOptionDto>
{
    public CreateServiceOptionValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters");

        RuleFor(x => x.PriceDeltaAmount)
            .GreaterThanOrEqualTo(0).WithMessage("Price delta must be non-negative");

        RuleFor(x => x.PriceDeltaCurrency)
            .NotEmpty().WithMessage("Currency is required")
            .MaximumLength(3).WithMessage("Currency must not exceed 3 characters");

        RuleFor(x => x.DurationDeltaMinutes)
            .InclusiveBetween(0, 480).WithMessage("Duration delta must be between 0 and 480 minutes");
    }
}
