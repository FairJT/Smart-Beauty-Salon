using Microsoft.EntityFrameworkCore;
using SmartSalon.Data;
using SmartSalon.DTOs;
using SmartSalon.Models;

namespace SmartSalon.Services
{
    public class ServiceService : IServiceService
    {
        private readonly ApplicationDbContext _db;

        public ServiceService(ApplicationDbContext db) => _db = db;

        public async Task<List<ServiceListItemDto>> GetServicesBySalonAsync(int salonId)
        {
            return await _db.SalonServices
                .Where(s => s.SalonId == salonId && s.IsActive)
                .Select(s => new ServiceListItemDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    Category = s.Category,
                    BaseDurationMinutes = s.BaseDurationMinutes,
                    BasePrice = s.BasePrice
                })
                .ToListAsync();
        }

        public async Task<int?> CreateServiceAsync(CreateServiceDto dto)
        {
            if (!await _db.Salons.AnyAsync(s => s.Id == dto.SalonId))
                return null;

            var service = new Models.SalonService
            {
                Name = dto.Name,
                Category = dto.Category,
                BaseDurationMinutes = dto.DurationMinutes,
                BasePrice = dto.Price,
                SalonId = dto.SalonId
            };

            _db.SalonServices.Add(service);
            await _db.SaveChangesAsync();
            return service.Id;
        }

        public async Task<bool> UpdateServiceAsync(int id, UpdateServiceDto dto)
        {
            var service = await _db.SalonServices.FindAsync(id);
            if (service == null) return false;

            service.Name = dto.Name;
            service.Category = dto.Category;
            service.BaseDurationMinutes = dto.DurationMinutes;
            service.BasePrice = dto.Price;

            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<int?> GetSalonIdAsync(int serviceId)
        {
            var service = await _db.SalonServices.FindAsync(serviceId);
            return service?.SalonId;
        }

        public async Task<(bool Success, string Message)> DeleteServiceAsync(int id)
        {
            var service = await _db.SalonServices.FindAsync(id);
            if (service == null) return (false, "Service not found");

            var hasActiveAppointments = await _db.Appointments.AnyAsync(a =>
                a.ServiceId == id &&
                a.Status != AppointmentStatus.Completed &&
                a.Status != AppointmentStatus.Cancelled);

            if (hasActiveAppointments)
                return (false, "Cannot delete service with active appointments");

            service.IsActive = false;
            await _db.SaveChangesAsync();
            return (true, "Service deleted");
        }
    }
}
