using System.ComponentModel.DataAnnotations;

namespace SalonOS.Catalog.Application.DTOs;

public class CreateCatalogServiceDto
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [Required]
    public Guid ServiceTypeId { get; set; }

    [Required]
    [Range(5, 480)]
    public int BaseDurationMinutes { get; set; }

    [Required]
    public long BasePriceAmount { get; set; }

    [Required]
    [MaxLength(3)]
    public string BasePriceCurrency { get; set; } = "IRR";
}

public class UpdateCatalogServiceDto
{
    [MaxLength(100)]
    public string? Name { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    public Guid? ServiceTypeId { get; set; }

    [Range(5, 480)]
    public int? BaseDurationMinutes { get; set; }

    public long? BasePriceAmount { get; set; }

    [MaxLength(3)]
    public string? BasePriceCurrency { get; set; }
}

public class CatalogServiceDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid ServiceTypeId { get; set; }
    public string ServiceTypeName { get; set; } = string.Empty;
    public int BaseDurationMinutes { get; set; }
    public long BasePriceAmount { get; set; }
    public string BasePriceCurrency { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<ServiceOptionDto> Options { get; set; } = new();
}

public class ServiceOptionDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public long PriceDeltaAmount { get; set; }
    public string PriceDeltaCurrency { get; set; } = string.Empty;
    public int DurationDeltaMinutes { get; set; }
    public bool IsActive { get; set; }
}

public class CreateServiceOptionDto
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    public long PriceDeltaAmount { get; set; }

    [MaxLength(3)]
    public string PriceDeltaCurrency { get; set; } = "IRR";

    public int DurationDeltaMinutes { get; set; }
}

public class MaterialDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public long PriceAmount { get; set; }
    public string PriceCurrency { get; set; } = string.Empty;
    public string? Unit { get; set; }
    public bool IsActive { get; set; }
}

public class CreateMaterialDto
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    public long PriceAmount { get; set; }

    [MaxLength(3)]
    public string PriceCurrency { get; set; } = "IRR";

    [MaxLength(50)]
    public string? Unit { get; set; }
}

public class ServiceTypeDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
}

public class CreateServiceTypeDto
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Category { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }
}
