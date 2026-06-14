using FluentValidation;
using SalonOS.Marketplace.Application.DTOs;

namespace SalonOS.Marketplace.Application.Validators;

/// <summary>
/// Validator for CreateServiceTemplateDto.
/// </summary>
public class CreateServiceTemplateValidator : AbstractValidator<CreateServiceTemplateDto>
{
    public CreateServiceTemplateValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters");

        RuleFor(x => x.Category)
            .NotEmpty().WithMessage("Category is required")
            .MaximumLength(100).WithMessage("Category must not exceed 100 characters");
    }
}
