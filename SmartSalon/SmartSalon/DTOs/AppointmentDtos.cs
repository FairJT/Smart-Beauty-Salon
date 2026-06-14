using System.ComponentModel.DataAnnotations;

namespace SmartSalon.DTOs
{
    public class CreateAppointmentDto
    {
        [Required]
        public int ArtistId { get; set; }

        [Required]
        public int SalonId { get; set; }

        [Required]
        public int ServiceId { get; set; }

        [Required(ErrorMessage = "زمان شروع الزامی است")]
        public DateTime StartTime { get; set; }

        [Required]
        [Range(5, 480, ErrorMessage = "مدت زمان بین ۵ تا ۴۸۰ دقیقه باشد")]
        public int DurationMinutes { get; set; }

        [Required]
        [Range(0, 99999999)]
        public decimal EstimatedPrice { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }
    }

    public class RateRequestDto
    {
        [Required]
        [Range(1, 5, ErrorMessage = "امتیاز باید بین ۱ تا ۵ باشد")]
        public int Rating { get; set; }

        [MaxLength(500)]
        public string? Comment { get; set; }
    }

    public class AppointmentListItemDto
    {
        public int Id { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int Status { get; set; }
        public decimal EstimatedPrice { get; set; }
        public decimal DepositAmount { get; set; }
        public bool IsRated { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public string SalonName { get; set; } = string.Empty;
        public string ArtistName { get; set; } = string.Empty;
        public string ServiceName { get; set; } = string.Empty;
    }

    public class SlotDto
    {
        public string Start { get; set; } = string.Empty;
        public string End { get; set; } = string.Empty;
        public DateTime StartFull { get; set; }
    }

    public class SlotsResponseDto
    {
        public string Date { get; set; } = string.Empty;
        public int ArtistId { get; set; }
        public int Duration { get; set; }
        public List<SlotDto> Slots { get; set; } = new();
    }

    public class CreateAppointmentResponseDto
    {
        public string Message { get; set; } = string.Empty;
        public int Id { get; set; }
        public decimal Deposit { get; set; }
    }

    public class PaginationParams
    {
        private const int MaxPageSize = 50;
        private int _pageSize = 10;

        public int Page { get; set; } = 1;

        public int Size
        {
            get => _pageSize;
            set => _pageSize = Math.Min(value, MaxPageSize);
        }
    }
}
