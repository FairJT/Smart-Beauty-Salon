namespace SmartSalon.Models
{
    public class Appointment
    {
        public int Id { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int DurationMinutes { get; set; }
        public decimal EstimatedPrice { get; set; }
        public decimal? FinalPrice { get; set; }
        public decimal DepositAmount { get; set; }
        public AppointmentStatus Status { get; set; } = AppointmentStatus.Pending;
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // این رزرو برای کدام مشتری است؟
        public string ClientId { get; set; } = string.Empty;
        public ApplicationUser? Client { get; set; }

        // این رزرو برای کدام هنرمند است؟
        public int ArtistId { get; set; }
        public Artist? Artist { get; set; }

        // این رزرو در کدام سالن است؟
        public int SalonId { get; set; }
        public Salon? Salon { get; set; }

        // این رزرو برای کدام خدمت است؟
        public int ServiceId { get; set; }
        public SalonService? Service { get; set; }

        public bool IsRated { get; set; } = false;
        public int Rating { get; set; } = 0;
        public string? Comment { get; set; }
        public bool ReminderSent { get; set; } = false;
    }

    public enum AppointmentStatus
    {
        Pending = 1,  // در انتظار تایید
        Confirmed = 2,  // تایید شده
        InProgress = 3,  // در حال انجام
        Completed = 4,  // تمام شده
        Cancelled = 5,  // لغو شده
        NoShow = 6   // مشتری نیامد
    }
}