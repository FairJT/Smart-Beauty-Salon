using FluentValidation;
using SalonOS.Identity.Application.DTOs;

namespace SalonOS.Identity.Application.Validators;

/// <summary>
/// Validator for CreateMembershipDto.
/// </summary>
public class CreateMembershipValidator : AbstractValidator<CreateMembershipDto>
{
    public CreateMembershipValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required");

        RuleFor(x => x.TenantId)
            .NotEmpty().WithMessage("TenantId is required");

        RuleFor(x => x.Role)
            .IsInEnum().WithMessage("Invalid role");
    }
}
