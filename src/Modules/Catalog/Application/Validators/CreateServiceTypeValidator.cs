using FluentValidation;
using SalonOS.Catalog.Application.DTOs;

namespace SalonOS.Catalog.Application.Validators;

public class CreateServiceTypeValidator : AbstractValidator<CreateServiceTypeDto>
{
    public CreateServiceTypeValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters");

        RuleFor(x => x.Category)
            .NotEmpty().WithMessage("Category is required")
            .MaximumLength(100).WithMessage("Category must not exceed 100 characters");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters");
    }
}
