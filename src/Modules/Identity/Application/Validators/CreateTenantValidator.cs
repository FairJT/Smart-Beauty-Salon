using FluentValidation;
using SalonOS.Identity.Application.DTOs;

namespace SalonOS.Identity.Application.Validators;

/// <summary>
/// Validator for CreateTenantDto.
/// </summary>
public class CreateTenantValidator : AbstractValidator<CreateTenantDto>
{
    public CreateTenantValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters");

        RuleFor(x => x.Slug)
            .NotEmpty().WithMessage("Slug is required")
            .Matches(@"^[a-z0-9-]+$").WithMessage("Slug must be lowercase letters, numbers, and hyphens")
            .MaximumLength(100).WithMessage("Slug must not exceed 100 characters");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters");

        RuleFor(x => x.ThemeColor)
            .MaximumLength(7).WithMessage("ThemeColor must not exceed 7 characters");

        RuleFor(x => x.Region)
            .NotEmpty().WithMessage("Region is required")
            .MaximumLength(2).WithMessage("Region must not exceed 2 characters");
    }
}
