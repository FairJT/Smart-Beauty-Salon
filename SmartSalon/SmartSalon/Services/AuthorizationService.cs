using Microsoft.EntityFrameworkCore;
using SmartSalon.Data;
using SmartSalon.Models;

namespace SmartSalon.Services
{
    public class AuthorizationService : IAuthorizationService
    {
        private readonly ApplicationDbContext _db;
        public AuthorizationService(ApplicationDbContext db) => _db = db;

        public async Task<bool> IsSuperAdminAsync(string userId)
        {
            var user = await _db.Users.FindAsync(userId);
            return user?.UserType == UserType.SuperAdmin;
        }

        public async Task<bool> IsSalonManagerOrAboveAsync(string userId)
        {
            var user = await _db.Users.FindAsync(userId);
            return user?.UserType == UserType.SuperAdmin || user?.UserType == UserType.SalonManager;
        }

        public async Task<int> GetUserTypeAsync(string userId)
        {
            var user = await _db.Users.FindAsync(userId);
            return (int)(user?.UserType ?? UserType.Client);
        }

        public async Task<bool> CanManageSalonAsync(string userId, int salonId)
        {
            var user = await _db.Users.FindAsync(userId);
            if (user == null) return false;
            if (user.UserType == UserType.SuperAdmin) return true;
            return await _db.Salons.AnyAsync(s => s.Id == salonId && s.ManagerId == userId);
        }
    }
}
