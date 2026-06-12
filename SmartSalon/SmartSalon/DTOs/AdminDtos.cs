namespace SmartSalon.DTOs
{
    public class AdminUserDto
    {
        public string Id { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string UserType { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public int LoyaltyPoints { get; set; }
        public int TotalVisits { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class ChangeUserTypeDto
    {
        public int UserType { get; set; }
    }

    public class AdminSalonDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public bool IsVip { get; set; }
        public bool IsActive { get; set; }
        public string ManagerId { get; set; } = string.Empty;
        public string ManagerName { get; set; } = string.Empty;
        public int ArtistCount { get; set; }
        public int ServiceCount { get; set; }
    }

    public class AdminStatsDto
    {
        public int TotalUsers { get; set; }
        public int TotalSalons { get; set; }
        public int TotalAppointments { get; set; }
        public int ActiveSalons { get; set; }
        public int TotalArtists { get; set; }
        public double TotalRevenue { get; set; }
    }

    public class CreateSalonByAdminDto
    {
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public string? Description { get; set; }
        public string ManagerId { get; set; } = string.Empty;
    }
}
