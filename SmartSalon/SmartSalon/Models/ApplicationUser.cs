using Microsoft.AspNetCore.Identity;

namespace SmartSalon.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string NationalCode { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }
        public UserType UserType { get; set; } = UserType.Client;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int LoyaltyPoints { get; set; } = 0;
        public int TotalVisits { get; set; } = 0;
    }

    public enum UserType
    {
        SuperAdmin = 1,
        SalonManager = 2,
        Artist = 3,
        Client = 4
    }
}