using System.ComponentModel.DataAnnotations;

namespace SalonOS.Identity.Application.DTOs;

/// <summary>
/// DTO for user registration.
/// </summary>
public class RegisterDto
{
    [Required]
    [RegularExpression(@"^09\d{9}$", ErrorMessage = "Invalid mobile number")]
    public string Mobile { get; set; } = string.Empty;

    [Required]
    [MinLength(8)]
    public string Password { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [RegularExpression(@"^\d{10}$", ErrorMessage = "Invalid national code")]
    public string NationalCode { get; set; } = string.Empty;
}

/// <summary>
/// DTO for user login.
/// </summary>
public class LoginDto
{
    [Required]
    [RegularExpression(@"^09\d{9}$", ErrorMessage = "Invalid mobile number")]
    public string Mobile { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}

/// <summary>
/// DTO for password change.
/// </summary>
public class ChangePasswordDto
{
    [Required]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required]
    [MinLength(8)]
    public string NewPassword { get; set; } = string.Empty;
}

/// <summary>
/// DTO for authentication response.
/// </summary>
public class AuthResponseDto
{
    public string Token { get; set; } = string.Empty;
    public int ExpiresIn { get; set; }
    public UserProfileDto User { get; set; } = null!;
}

/// <summary>
/// DTO for user profile.
/// </summary>
public class UserProfileDto
{
    public string Id { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Mobile { get; set; } = string.Empty;
    public string UserType { get; set; } = string.Empty;
    public int LoyaltyPoints { get; set; }
    public int TotalVisits { get; set; }
    public bool IsActive { get; set; }
}
