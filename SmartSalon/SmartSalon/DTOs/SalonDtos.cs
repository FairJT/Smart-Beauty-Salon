using System.ComponentModel.DataAnnotations;

namespace SmartSalon.DTOs
{
    public class CreateSalonDto
    {
        [Required(ErrorMessage = "نام سالن الزامی است")]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "آدرس slug الزامی است")]
        [RegularExpression(@"^[a-z0-9\-]+$", ErrorMessage = "slug فقط شامل حروف کوچک انگلیسی، اعداد و خط تیره باشد")]
        [MaxLength(100)]
        public string Slug { get; set; } = string.Empty;

        [Phone(ErrorMessage = "شماره تلفن نامعتبر است")]
        [MaxLength(15)]
        public string? Phone { get; set; }

        [MaxLength(500)]
        public string? Address { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }

        [Required(ErrorMessage = "شناسه مدیر الزامی است")]
        public string ManagerId { get; set; } = string.Empty;
    }

    public class UpdateSalonDto
    {
        [Required(ErrorMessage = "نام سالن الزامی است")]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Phone(ErrorMessage = "شماره تلفن نامعتبر است")]
        [MaxLength(15)]
        public string? Phone { get; set; }

        [MaxLength(500)]
        public string? Address { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }

        [MaxLength(7)]
        public string? ThemeColor { get; set; }
    }

    public class SalonListItemDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? LogoUrl { get; set; }
        public double RatingAvg { get; set; }
        public bool IsVip { get; set; }
        public string? Address { get; set; }
        public int ServiceCount { get; set; }
        public int ArtistCount { get; set; }
    }

    public class SalonDetailDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public string? Description { get; set; }
        public string? LogoUrl { get; set; }
        public string ThemeColor { get; set; } = "#1B3A5C";
        public bool IsVip { get; set; }
        public double RatingAvg { get; set; }
        public List<ArtistListItemDto> Artists { get; set; } = new();
        public List<ServiceListItemDto> Services { get; set; } = new();
    }

    public class PaginatedResult<T>
    {
        public int Total { get; set; }
        public int Page { get; set; }
        public int Size { get; set; }
        public List<T> Data { get; set; } = new();
    }

    public class ArtistListItemDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? PhotoUrl { get; set; }
        public string? BioShort { get; set; }
        public decimal RatingAvg { get; set; }
        public int RatingCount { get; set; }
        public string ContractType { get; set; } = string.Empty;
    }

    public class ServiceListItemDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public int BaseDurationMinutes { get; set; }
        public decimal BasePrice { get; set; }
    }
}
