using SmartSalon.DTOs;

namespace SmartSalon.Services
{
    public interface IAuthService
    {
        Task<AuthResponseDto?> RegisterAsync(RegisterDto dto);
        Task<AuthResponseDto?> LoginAsync(LoginDto dto);
        Task<UserProfileDto?> GetProfileAsync(string userId);
        Task<(bool Success, string Message)> ChangePasswordAsync(string userId, ChangePasswordDto dto);
    }
}
