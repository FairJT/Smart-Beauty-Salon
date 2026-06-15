using FluentValidation;
using SalonOS.Inventory.Application.DTOs;

namespace SalonOS.Inventory.Application.Validators;

public class CreateStockMovementValidator : AbstractValidator<CreateStockMovementDto>
{
    public CreateStockMovementValidator()
    {
        RuleFor(x => x.InventoryItemId)
            .NotEmpty().WithMessage("Inventory item ID is required");

        RuleFor(x => x.Type)
            .NotEmpty().WithMessage("Type is required")
            .MaximumLength(50).WithMessage("Type must not exceed 50 characters")
            .Must(t => t is "In" or "Out" or "Adjustment")
            .WithMessage("Type must be 'In', 'Out', or 'Adjustment'");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than 0");

        RuleFor(x => x.Reference)
            .MaximumLength(100).WithMessage("Reference must not exceed 100 characters");

        RuleFor(x => x.Notes)
            .MaximumLength(500).WithMessage("Notes must not exceed 500 characters");
    }
}
