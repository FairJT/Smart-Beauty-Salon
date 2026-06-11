using SmartSalon.DTOs;

namespace SmartSalon.Services
{
    public interface IAppointmentService
    {
        Task<SlotsResponseDto?> GetSlotsAsync(int artistId, DateTime date, int duration);
        Task<CreateAppointmentResponseDto?> CreateAsync(CreateAppointmentDto dto, string clientId);
        Task<List<AppointmentListItemDto>> GetMineAsync(string clientId);
        Task<(bool Success, bool IsNotFound)> ConfirmAsync(int id, string userId);
        Task<bool> CompleteAsync(int id, string userId);
        Task<bool> CancelAsync(int id, string userId);
        Task<(bool Success, string Message, decimal? RatingAvg)> RateAsync(int id, string userId, RateRequestDto request);
    }
}
