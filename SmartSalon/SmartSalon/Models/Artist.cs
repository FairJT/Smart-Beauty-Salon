namespace SmartSalon.Models
{
    public class Artist
    {
        public int Id { get; set; }
        public string BioShort { get; set; } = string.Empty;
        public string? BioLong { get; set; }
        public string Skill { get; set; } = string.Empty;
        public ContractType ContractType { get; set; }
        public decimal RatingAvg { get; set; } = 0;
        public int RatingCount { get; set; } = 0;

        public string? PhotoUrl { get; set; }
        public bool IsActive { get; set; } = true;

        // این هنرمند کدام کاربر است؟
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser? User { get; set; }

        // این هنرمند در کدام سالن کار می‌کند؟
        public int SalonId { get; set; }
        public Salon? Salon { get; set; }

        // رزروهای این هنرمند
        public List<Appointment> Appointments { get; set; } = new();
    }

    public enum ContractType
    {
        FixedSalary = 1,  // حقوق ثابت
        LineRent = 2,  // اجاره لاین
        RoomRent = 3   // اجاره اتاق
    }
}