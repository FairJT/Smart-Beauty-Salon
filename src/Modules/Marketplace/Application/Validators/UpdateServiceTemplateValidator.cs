using FluentValidation;
using SalonOS.Marketplace.Application.DTOs;

namespace SalonOS.Marketplace.Application.Validators;

public class UpdateServiceTemplateValidator : AbstractValidator<UpdateServiceTemplateDto>
{
    public UpdateServiceTemplateValidator()
    {
        RuleFor(x => x.Name)
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters");

        RuleFor(x => x.Category)
            .MaximumLength(100).WithMessage("Category must not exceed 100 characters");
    }
}
