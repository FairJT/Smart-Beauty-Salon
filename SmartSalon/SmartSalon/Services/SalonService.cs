using Microsoft.EntityFrameworkCore;
using SmartSalon.Data;
using SmartSalon.DTOs;
using SmartSalon.Models;

namespace SmartSalon.Services
{
    public class SalonService : ISalonService
    {
        private readonly ApplicationDbContext _db;

        public SalonService(ApplicationDbContext db) => _db = db;

        public async Task<PaginatedResult<SalonListItemDto>> GetSalonsAsync(
            string? search, string? service, bool? vipOnly, int page, int size)
        {
            var query = _db.Salons
                .Include(s => s.Services.Where(sv => sv.IsActive))
                .Include(s => s.Artists.Where(a => a.IsActive))
                .Where(s => s.IsActive);

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(s => s.Name.Contains(search));

            if (!string.IsNullOrWhiteSpace(service))
                query = query.Where(s => s.Services.Any(sv =>
                    sv.Name.Contains(service) && sv.IsActive));

            if (vipOnly == true)
                query = query.Where(s => s.IsVip);

            var total = await query.CountAsync();

            var data = await query
                .OrderByDescending(s => s.IsVip)
                .ThenByDescending(s => s.RatingAvg)
                .Skip((page - 1) * size)
                .Take(size)
                .Select(s => new SalonListItemDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    LogoUrl = s.LogoUrl,
                    RatingAvg = s.RatingAvg,
                    IsVip = s.IsVip,
                    Address = s.Address,
                    ServiceCount = s.Services.Count,
                    ArtistCount = s.Artists.Count
                })
                .ToListAsync();

            return new PaginatedResult<SalonListItemDto>
            {
                Total = total,
                Page = page,
                Size = size,
                Data = data
            };
        }

        public async Task<SalonDetailDto?> GetSalonByIdAsync(int id)
        {
            var salon = await _db.Salons
                .Include(s => s.Artists.Where(a => a.IsActive)).ThenInclude(a => a.User)
                .Include(s => s.Services.Where(sv => sv.IsActive))
                .FirstOrDefaultAsync(s => s.Id == id && s.IsActive);

            if (salon == null) return null;

            return new SalonDetailDto
            {
                Id = salon.Id,
                Name = salon.Name,
                Slug = salon.Slug,
                Phone = salon.Phone,
                Address = salon.Address,
                Description = salon.Description,
                LogoUrl = salon.LogoUrl,
                ThemeColor = salon.ThemeColor,
                IsVip = salon.IsVip,
                RatingAvg = salon.RatingAvg,
                Artists = salon.Artists.Select(a => new ArtistListItemDto
                {
                    Id = a.Id,
                    FirstName = a.User?.FirstName ?? "",
                    LastName = a.User?.LastName ?? "",
                    PhotoUrl = a.PhotoUrl,
                    BioShort = a.BioShort,
                    RatingAvg = a.RatingAvg,
                    RatingCount = a.RatingCount,
                    ContractType = a.ContractType.ToString()
                }).ToList(),
                Services = salon.Services.Select(s => new ServiceListItemDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    Category = s.Category,
                    BaseDurationMinutes = s.BaseDurationMinutes,
                    BasePrice = s.BasePrice
                }).ToList()
            };
        }

        public async Task<int> CreateSalonAsync(CreateSalonDto dto)
        {
            var salon = new Salon
            {
                Name = dto.Name,
                Slug = dto.Slug,
                Phone = dto.Phone,
                Address = dto.Address,
                Description = dto.Description,
                ManagerId = dto.ManagerId ?? string.Empty
            };

            _db.Salons.Add(salon);
            await _db.SaveChangesAsync();
            return salon.Id;
        }

        public async Task<bool> UpdateSalonAsync(int id, UpdateSalonDto dto, string userId)
        {
            var salon = await _db.Salons.FindAsync(id);
            if (salon == null) return false;

            salon.Name = dto.Name;
            salon.Phone = dto.Phone;
            salon.Address = dto.Address;
            salon.Description = dto.Description;

            if (!string.IsNullOrEmpty(dto.ThemeColor))
                salon.ThemeColor = dto.ThemeColor;

            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteSalonAsync(int id, string userId)
        {
            var salon = await _db.Salons.FindAsync(id);
            if (salon == null) return false;

            salon.IsActive = false;
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> IsSalonManagerAsync(int salonId, string userId)
        {
            var user = await _db.Users.FindAsync(userId);
            if (user?.UserType == UserType.SuperAdmin) return true;
            return await _db.Salons.AnyAsync(s => s.Id == salonId && s.ManagerId == userId);
        }

        public async Task<List<SalonListItemDto>> GetAllSalonsForAdminAsync()
        {
            return await _db.Salons
                .Include(s => s.Services.Where(sv => sv.IsActive))
                .Include(s => s.Artists.Where(a => a.IsActive))
                .Select(s => new SalonListItemDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    LogoUrl = s.LogoUrl,
                    RatingAvg = s.RatingAvg,
                    IsVip = s.IsVip,
                    Address = s.Address,
                    ServiceCount = s.Services.Count,
                    ArtistCount = s.Artists.Count
                })
                .OrderByDescending(s => s.IsVip)
                .ThenByDescending(s => s.RatingAvg)
                .ToListAsync();
        }
    }
}
