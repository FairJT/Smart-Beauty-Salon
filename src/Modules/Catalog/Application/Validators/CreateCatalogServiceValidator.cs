using FluentValidation;
using SalonOS.Catalog.Application.DTOs;

namespace SalonOS.Catalog.Application.Validators;

public class CreateCatalogServiceValidator : AbstractValidator<CreateCatalogServiceDto>
{
    public CreateCatalogServiceValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters");

        RuleFor(x => x.ServiceTypeId)
            .NotEmpty().WithMessage("ServiceTypeId is required");

        RuleFor(x => x.BaseDurationMinutes)
            .InclusiveBetween(5, 480).WithMessage("Duration must be between 5 and 480 minutes");

        RuleFor(x => x.BasePriceAmount)
            .GreaterThan(0).WithMessage("Price must be greater than 0");

        RuleFor(x => x.BasePriceCurrency)
            .NotEmpty().WithMessage("Currency is required")
            .MaximumLength(3).WithMessage("Currency must not exceed 3 characters");
    }
}
