namespace SmartSalon.Models
{
    public class Salon
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string? Description { get; set; }
        public string? LogoUrl { get; set; }
        public string ThemeColor { get; set; } = "#1B3A5C";
        public string AdminTheme { get; set; } = "gold";
        public bool IsVip { get; set; } = false;
        public bool IsActive { get; set; } = true;
        public double RatingAvg { get; set; } = 0.0;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // مدیر این سالن کیست؟
        public string ManagerId { get; set; } = string.Empty;
        public ApplicationUser? Manager { get; set; }

        // پرسنل و خدمات این سالن
        public List<Artist> Artists { get; set; } = new();
        public List<SalonService> Services { get; set; } = new();
    }
}