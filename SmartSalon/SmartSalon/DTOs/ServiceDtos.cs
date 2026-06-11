using System.ComponentModel.DataAnnotations;

namespace SmartSalon.DTOs
{
    public class CreateServiceDto
    {
        [Required(ErrorMessage = "نام خدمت الزامی است")]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "دسته‌بندی الزامی است")]
        [MaxLength(50)]
        public string Category { get; set; } = string.Empty;

        [Required]
        [Range(5, 480, ErrorMessage = "مدت زمان بین ۵ تا ۴۸۰ دقیقه باشد")]
        public int DurationMinutes { get; set; }

        [Required]
        [Range(0, 99999999, ErrorMessage = "قیمت نامعتبر است")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "شناسه سالن الزامی است")]
        public int SalonId { get; set; }
    }

    public class UpdateServiceDto
    {
        [Required(ErrorMessage = "نام خدمت الزامی است")]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "دسته‌بندی الزامی است")]
        [MaxLength(50)]
        public string Category { get; set; } = string.Empty;

        [Required]
        [Range(5, 480)]
        public int DurationMinutes { get; set; }

        [Required]
        [Range(0, 99999999)]
        public decimal Price { get; set; }
    }
}
