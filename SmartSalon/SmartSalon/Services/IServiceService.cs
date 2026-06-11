using SmartSalon.DTOs;

namespace SmartSalon.Services
{
    public interface IServiceService
    {
        Task<List<ServiceListItemDto>> GetServicesBySalonAsync(int salonId);
        Task<int?> CreateServiceAsync(CreateServiceDto dto);
        Task<bool> UpdateServiceAsync(int id, UpdateServiceDto dto);
        Task<int?> GetSalonIdAsync(int serviceId);
        Task<(bool Success, string Message)> DeleteServiceAsync(int id);
    }
}
