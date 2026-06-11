using System.ComponentModel.DataAnnotations;

namespace SalonOS.Catalog.Application.DTOs;

/// <summary>
/// DTO for creating a catalog service.
/// </summary>
public class CreateCatalogServiceDto
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [Required]
    [MaxLength(100)]
    public string Category { get; set; } = string.Empty;

    [Required]
    [Range(5, 480)]
    public int BaseDurationMinutes { get; set; }

    [Required]
    public long BasePriceAmount { get; set; }

    [Required]
    [MaxLength(3)]
    public string BasePriceCurrency { get; set; } = "IRR";
}

/// <summary>
/// DTO for updating a catalog service.
/// </summary>
public class UpdateCatalogServiceDto
{
    [MaxLength(100)]
    public string? Name { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    [MaxLength(100)]
    public string? Category { get; set; }

    [Range(5, 480)]
    public int? BaseDurationMinutes { get; set; }

    public long? BasePriceAmount { get; set; }

    [MaxLength(3)]
    public string? BasePriceCurrency { get; set; }
}

/// <summary>
/// DTO for catalog service response.
/// </summary>
public class CatalogServiceDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Category { get; set; } = string.Empty;
    public int BaseDurationMinutes { get; set; }
    public long BasePriceAmount { get; set; }
    public string BasePriceCurrency { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<CatalogServiceOptionDto> Options { get; set; } = new();
}

/// <summary>
/// DTO for catalog service option.
/// </summary>
public class CatalogServiceOptionDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
}
