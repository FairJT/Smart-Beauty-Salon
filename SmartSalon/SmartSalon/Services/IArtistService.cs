using SmartSalon.DTOs;

namespace SmartSalon.Services
{
    public interface IArtistService
    {
        Task<List<ArtistListItemDto>> GetArtistsBySalonAsync(int salonId);
        Task<ArtistListItemDto?> GetByIdAsync(int id);
        Task<ArtistReportDto?> GetReportAsync(int id, DateTime? from, DateTime? to, int page = 1, int size = 30);
        Task<int?> CreateArtistAsync(CreateArtistDto dto);
        Task<bool> UpdateArtistAsync(int id, UpdateArtistDto dto);
        Task<int?> GetSalonIdAsync(int artistId);
        Task<(bool Success, string Message)> DeleteArtistAsync(int id);
        Task<bool> UploadPhotoAsync(int id, string photoUrl);
    }
}
