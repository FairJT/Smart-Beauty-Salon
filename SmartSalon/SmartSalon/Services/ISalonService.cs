using SmartSalon.DTOs;

namespace SmartSalon.Services
{
    public interface ISalonService
    {
        Task<PaginatedResult<SalonListItemDto>> GetSalonsAsync(
            string? search, string? service, bool? vipOnly, int page, int size);
        Task<SalonDetailDto?> GetSalonByIdAsync(int id);
        Task<int> CreateSalonAsync(CreateSalonDto dto);
        Task<bool> UpdateSalonAsync(int id, UpdateSalonDto dto, string userId);
        Task<bool> IsSalonManagerAsync(int salonId, string userId);
    }
}
