using System.ComponentModel.DataAnnotations;
using SmartSalon.Models;

namespace SmartSalon.DTOs
{
    public class CreateArtistDto
    {
        [Required(ErrorMessage = "شناسه کاربر الزامی است")]
        public string UserId { get; set; } = string.Empty;

        [Required(ErrorMessage = "شناسه سالن الزامی است")]
        public int SalonId { get; set; }

        [Required(ErrorMessage = "بیو کوتاه الزامی است")]
        [MaxLength(200)]
        public string BioShort { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? BioLong { get; set; }

        [Required]
        public ContractType ContractType { get; set; }
    }

    public class UpdateArtistDto
    {
        [Required(ErrorMessage = "بیو کوتاه الزامی است")]
        [MaxLength(200)]
        public string BioShort { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? BioLong { get; set; }

        [Required]
        public ContractType ContractType { get; set; }
    }

    public class ArtistReportDto
    {
        public string ArtistName { get; set; } = string.Empty;
        public string? PhotoUrl { get; set; }
        public decimal RatingAvg { get; set; }
        public int RatingCount { get; set; }
        public string FromDate { get; set; } = string.Empty;
        public string ToDate { get; set; } = string.Empty;
        public int TotalAppointments { get; set; }
        public int CompletedAppointments { get; set; }
        public int CancelledAppointments { get; set; }
        public decimal TotalRevenue { get; set; }
        public double AvgRating { get; set; }
        public List<DailyReportItem> DailyReport { get; set; } = new();
        public List<ServiceReportItem> ServiceReport { get; set; } = new();
    }

    public class DailyReportItem
    {
        public string Date { get; set; } = string.Empty;
        public int Count { get; set; }
        public decimal Revenue { get; set; }
    }

    public class ServiceReportItem
    {
        public string ServiceName { get; set; } = string.Empty;
        public int Count { get; set; }
        public decimal Revenue { get; set; }
    }
}
