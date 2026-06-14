using Microsoft.EntityFrameworkCore;
using SmartSalon.Data;
using SmartSalon.DTOs;
using SmartSalon.Models;

namespace SmartSalon.Services
{
    public class ArtistService : IArtistService
    {
        private readonly ApplicationDbContext _db;

        public ArtistService(ApplicationDbContext db) => _db = db;

        public async Task<List<ArtistListItemDto>> GetArtistsBySalonAsync(int salonId)
        {
            return await _db.Artists
                .Where(a => a.SalonId == salonId && a.IsActive)
                .Include(a => a.User)
                .Select(a => new ArtistListItemDto
                {
                    Id = a.Id,
                    FirstName = a.User!.FirstName,
                    LastName = a.User.LastName,
                    PhotoUrl = a.PhotoUrl,
                    BioShort = a.BioShort,
                    Skill = a.Skill,
                    RatingAvg = a.RatingAvg,
                    RatingCount = a.RatingCount,
                    ContractType = a.ContractType.ToString()
                })
                .ToListAsync();
        }

        public async Task<ArtistListItemDto?> GetByIdAsync(int id)
        {
            var artist = await _db.Artists.Include(a => a.User).FirstOrDefaultAsync(a => a.Id == id && a.IsActive);
            if (artist == null) return null;

            return new ArtistListItemDto
            {
                Id = artist.Id,
                FirstName = artist.User?.FirstName ?? "",
                LastName = artist.User?.LastName ?? "",
                PhotoUrl = artist.PhotoUrl,
                BioShort = artist.BioShort,
                Skill = artist.Skill,
                RatingAvg = artist.RatingAvg,
                RatingCount = artist.RatingCount,
                ContractType = artist.ContractType.ToString()
            };
        }

        public async Task<ArtistReportDto?> GetReportAsync(int id, DateTime? from, DateTime? to, int page = 1, int size = 30)
        {
            var artist = await _db.Artists.Include(a => a.User).FirstOrDefaultAsync(a => a.Id == id);
            if (artist == null) return null;

            var fromDate = from?.Date ?? DateTime.Today.AddDays(-30);
            var toDate = to?.Date ?? DateTime.Today;

            var query = _db.Appointments
                .Include(a => a.Service)
                .Where(a => a.ArtistId == id
                         && a.StartTime.Date >= fromDate
                         && a.StartTime.Date <= toDate);

            var totalCount = await query.CountAsync();

            var appointments = await query
                .OrderByDescending(a => a.StartTime)
                .Skip((page - 1) * size)
                .Take(size)
                .ToListAsync();

            var completed = appointments.Where(a => a.Status == AppointmentStatus.Completed).ToList();

            return new ArtistReportDto
            {
                ArtistName = (artist.User?.FirstName ?? "") + " " + (artist.User?.LastName ?? ""),
                PhotoUrl = artist.PhotoUrl,
                RatingAvg = artist.RatingAvg,
                RatingCount = artist.RatingCount,
                FromDate = fromDate.ToString("yyyy-MM-dd"),
                ToDate = toDate.ToString("yyyy-MM-dd"),
                TotalAppointments = totalCount,
                CompletedAppointments = completed.Count,
                CancelledAppointments = appointments.Count(a => a.Status == AppointmentStatus.Cancelled),
                TotalRevenue = completed.Sum(a => a.EstimatedPrice),
                AvgRating = completed.Where(a => a.IsRated).Select(a => (double)a.Rating).DefaultIfEmpty(0).Average(),
                DailyReport = completed
                    .GroupBy(a => a.StartTime.Date)
                    .Select(g => new DailyReportItem
                    {
                        Date = g.Key.ToString("yyyy-MM-dd"),
                        Count = g.Count(),
                        Revenue = g.Sum(a => a.EstimatedPrice)
                    })
                    .OrderBy(d => d.Date)
                    .ToList(),
                ServiceReport = completed
                    .GroupBy(a => a.Service?.Name ?? "Unknown")
                    .Select(g => new ServiceReportItem
                    {
                        ServiceName = g.Key,
                        Count = g.Count(),
                        Revenue = g.Sum(a => a.EstimatedPrice)
                    })
                    .OrderByDescending(s => s.Revenue)
                    .ToList()
            };
        }

        public async Task<int?> CreateArtistAsync(CreateArtistDto dto)
        {
            if (!await _db.Salons.AnyAsync(s => s.Id == dto.SalonId))
                return null;

            if (!await _db.Users.AnyAsync(u => u.Id == dto.UserId))
                return null;

            if (await _db.Artists.AnyAsync(a => a.UserId == dto.UserId && a.SalonId == dto.SalonId))
                return null;

            var artist = new Artist
            {
                UserId = dto.UserId,
                SalonId = dto.SalonId,
                BioShort = dto.BioShort,
                BioLong = dto.BioLong,
                Skill = dto.Skill,
                ContractType = dto.ContractType
            };

            _db.Artists.Add(artist);
            await _db.SaveChangesAsync();
            return artist.Id;
        }

        public async Task<bool> UpdateArtistAsync(int id, UpdateArtistDto dto)
        {
            var artist = await _db.Artists.FindAsync(id);
            if (artist == null) return false;

            artist.BioShort = dto.BioShort;
            artist.BioLong = dto.BioLong;
            artist.Skill = dto.Skill;
            artist.ContractType = dto.ContractType;

            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<int?> GetSalonIdAsync(int artistId)
        {
            var artist = await _db.Artists.FindAsync(artistId);
            return artist?.SalonId;
        }

        public async Task<(bool Success, string Message)> DeleteArtistAsync(int id)
        {
            var artist = await _db.Artists.FindAsync(id);
            if (artist == null) return (false, "Artist not found");

            var hasActiveAppointments = await _db.Appointments.AnyAsync(a =>
                a.ArtistId == id &&
                a.Status != AppointmentStatus.Completed &&
                a.Status != AppointmentStatus.Cancelled);

            if (hasActiveAppointments)
                return (false, "Cannot delete artist with active appointments");

            artist.IsActive = false;
            await _db.SaveChangesAsync();
            return (true, "Artist deleted");
        }

        public async Task<bool> UploadPhotoAsync(int id, string photoUrl)
        {
            var artist = await _db.Artists.FindAsync(id);
            if (artist == null) return false;

            artist.PhotoUrl = photoUrl;
            await _db.SaveChangesAsync();
            return true;
        }
    }
}
