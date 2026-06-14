using FluentValidation;
using SmartSalon.DTOs;

namespace SmartSalon.Validation
{
    public class CreateAppointmentValidator : AbstractValidator<CreateAppointmentDto>
    {
        public CreateAppointmentValidator()
        {
            RuleFor(x => x.ArtistId).GreaterThan(0).WithMessage("Artist ID is required");
            RuleFor(x => x.SalonId).GreaterThan(0).WithMessage("Salon ID is required");
            RuleFor(x => x.ServiceId).GreaterThan(0).WithMessage("Service ID is required");
            RuleFor(x => x.StartTime)
                .GreaterThan(DateTime.UtcNow.AddMinutes(-30))
                .WithMessage("Appointment time cannot be in the past");
            RuleFor(x => x.DurationMinutes)
                .InclusiveBetween(5, 480)
                .WithMessage("Duration must be between 5 and 480 minutes");
            RuleFor(x => x.EstimatedPrice)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Price cannot be negative");
        }
    }

    public class CreateSalonValidator : AbstractValidator<CreateSalonDto>
    {
        public CreateSalonValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Salon name is required")
                .MaximumLength(100).WithMessage("Name cannot exceed 100 characters");
            RuleFor(x => x.Slug)
                .NotEmpty().WithMessage("Slug is required")
                .Matches(@"^[a-z0-9\-]+$").WithMessage("Slug must contain only lowercase letters, numbers, and hyphens")
                .MaximumLength(100).WithMessage("Slug cannot exceed 100 characters");
            RuleFor(x => x.ManagerId)
                .NotEmpty().WithMessage("Manager ID is required");
            RuleFor(x => x.Phone)
                .Matches(@"^09\d{9}$").When(x => !string.IsNullOrEmpty(x.Phone))
                .WithMessage("Invalid phone number format");
        }
    }

    public class RegisterValidator : AbstractValidator<RegisterDto>
    {
        public RegisterValidator()
        {
            RuleFor(x => x.Mobile)
                .NotEmpty().WithMessage("Mobile is required")
                .Matches(@"^09\d{9}$").WithMessage("Invalid mobile number");
            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required")
                .MinimumLength(8).WithMessage("Password must be at least 8 characters")
                .Matches(@"[A-Za-z]").WithMessage("Password must contain at least one letter")
                .Matches(@"\d").WithMessage("Password must contain at least one digit");
            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("First name is required")
                .MaximumLength(50).WithMessage("First name cannot exceed 50 characters");
            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Last name is required")
                .MaximumLength(50).WithMessage("Last name cannot exceed 50 characters");
            RuleFor(x => x.NationalCode)
                .NotEmpty().WithMessage("National code is required")
                .Matches(@"^\d{10}$").WithMessage("National code must be exactly 10 digits");
        }
    }
}
