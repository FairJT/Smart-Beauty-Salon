namespace SmartSalon.Services
{
    public interface IAuthorizationService
    {
        Task<bool> IsSuperAdminAsync(string userId);
        Task<bool> IsSalonManagerOrAboveAsync(string userId);
        Task<int> GetUserTypeAsync(string userId);
        Task<bool> CanManageSalonAsync(string userId, int salonId);
    }
}
