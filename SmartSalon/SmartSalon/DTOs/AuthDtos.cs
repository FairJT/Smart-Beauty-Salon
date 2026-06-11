using System.ComponentModel.DataAnnotations;

namespace SmartSalon.DTOs
{
    public class RegisterDto
    {
        [Required(ErrorMessage = "Mobile is required")]
        [RegularExpression(@"^09\d{9}$", ErrorMessage = "Invalid mobile number")]
        public string Mobile { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "First name is required")]
        [MaxLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required")]
        [MaxLength(50)]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "National code is required")]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "National code must be 10 digits")]
        public string NationalCode { get; set; } = string.Empty;
    }

    public class LoginDto
    {
        [Required(ErrorMessage = "Mobile is required")]
        public string Mobile { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; } = string.Empty;
    }

    public class ChangePasswordDto
    {
        [Required(ErrorMessage = "Current password is required")]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "New password is required")]
        [MinLength(8, ErrorMessage = "New password must be at least 8 characters")]
        public string NewPassword { get; set; } = string.Empty;
    }

    public class AuthResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public int ExpiresIn { get; set; }
        public UserProfileDto User { get; set; } = null!;
    }

    public class UserProfileDto
    {
        public string Id { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? Mobile { get; set; }
        public string UserType { get; set; } = string.Empty;
        public int LoyaltyPoints { get; set; }
        public int TotalVisits { get; set; }
        public bool IsActive { get; set; }
    }
}
