using FluentValidation;
using SalonOS.Inventory.Application.DTOs;

namespace SalonOS.Inventory.Application.Validators;

/// <summary>
/// Validator for CreateInventoryItemDto.
/// </summary>
public class CreateInventoryItemValidator : AbstractValidator<CreateInventoryItemDto>
{
    public CreateInventoryItemValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters");

        RuleFor(x => x.Category)
            .NotEmpty().WithMessage("Category is required")
            .MaximumLength(100).WithMessage("Category must not exceed 100 characters");

        RuleFor(x => x.Sku)
            .MaximumLength(50).WithMessage("SKU must not exceed 50 characters");

        RuleFor(x => x.OnHandQty)
            .GreaterThanOrEqualTo(0).WithMessage("On-hand quantity must be non-negative");

        RuleFor(x => x.ReorderThreshold)
            .GreaterThanOrEqualTo(0).WithMessage("Reorder threshold must be non-negative");

        RuleFor(x => x.UnitCostAmount)
            .GreaterThan(0).WithMessage("Unit cost must be greater than 0");

        RuleFor(x => x.UnitCostCurrency)
            .NotEmpty().WithMessage("Currency is required")
            .MaximumLength(3).WithMessage("Currency must not exceed 3 characters");
    }
}
