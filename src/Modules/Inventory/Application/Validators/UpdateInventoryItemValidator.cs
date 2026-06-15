using FluentValidation;
using SalonOS.Inventory.Application.DTOs;

namespace SalonOS.Inventory.Application.Validators;

public class UpdateInventoryItemValidator : AbstractValidator<UpdateInventoryItemDto>
{
    public UpdateInventoryItemValidator()
    {
        RuleFor(x => x.Name)
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters");

        RuleFor(x => x.Category)
            .MaximumLength(100).WithMessage("Category must not exceed 100 characters");

        RuleFor(x => x.Sku)
            .MaximumLength(50).WithMessage("SKU must not exceed 50 characters");

        RuleFor(x => x.ReorderThreshold)
            .GreaterThanOrEqualTo(0).WithMessage("Reorder threshold must be non-negative");

        When(x => x.UnitCostAmount.HasValue, () =>
        {
            RuleFor(x => x.UnitCostAmount!)
                .GreaterThan(0).WithMessage("Unit cost must be greater than 0");
        });

        When(x => x.UnitCostCurrency != null, () =>
        {
            RuleFor(x => x.UnitCostCurrency!)
                .NotEmpty().WithMessage("Currency is required")
                .MaximumLength(3).WithMessage("Currency must not exceed 3 characters");
        });
    }
}
