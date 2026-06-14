namespace SmartSalon.Models
{
    public class SalonService
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public int BaseDurationMinutes { get; set; } = 30;
        public decimal BasePrice { get; set; }
        public bool IsActive { get; set; } = true;

        // این خدمت متعلق به کدام سالن است؟
        public int SalonId { get; set; }
        public Salon? Salon { get; set; }
    }
}