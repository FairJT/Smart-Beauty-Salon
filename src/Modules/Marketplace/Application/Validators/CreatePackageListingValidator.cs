using FluentValidation;
using SalonOS.Marketplace.Application.DTOs;

namespace SalonOS.Marketplace.Application.Validators;

/// <summary>
/// Validator for CreatePackageListingDto.
/// </summary>
public class CreatePackageListingValidator : AbstractValidator<CreatePackageListingDto>
{
    public CreatePackageListingValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters");

        RuleFor(x => x.Category)
            .NotEmpty().WithMessage("Category is required")
            .MaximumLength(100).WithMessage("Category must not exceed 100 characters");

        RuleFor(x => x.PriceAmount)
            .GreaterThan(0).WithMessage("Price must be greater than 0");

        RuleFor(x => x.PriceCurrency)
            .NotEmpty().WithMessage("Currency is required")
            .MaximumLength(3).WithMessage("Currency must not exceed 3 characters");

        RuleFor(x => x.DurationMonths)
            .InclusiveBetween(1, 120).WithMessage("Duration must be between 1 and 120 months");
    }
}
