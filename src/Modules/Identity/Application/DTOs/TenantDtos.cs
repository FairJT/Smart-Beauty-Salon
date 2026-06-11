using System.ComponentModel.DataAnnotations;

namespace SalonOS.Identity.Application.DTOs;

/// <summary>
/// DTO for creating a tenant.
/// </summary>
public class CreateTenantDto
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [RegularExpression(@"^[a-z0-9-]+$", ErrorMessage = "Slug must be lowercase letters, numbers, and hyphens")]
    [MaxLength(100)]
    public string Slug { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    public string? LogoUrl { get; set; }

    [MaxLength(7)]
    public string? ThemeColor { get; set; }

    [MaxLength(2)]
    public string Region { get; set; } = "IR";
}

/// <summary>
/// DTO for updating a tenant.
/// </summary>
public class UpdateTenantDto
{
    [MaxLength(100)]
    public string? Name { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    public string? LogoUrl { get; set; }

    [MaxLength(7)]
    public string? ThemeColor { get; set; }

    [MaxLength(2)]
    public string? Region { get; set; }
}

/// <summary>
/// DTO for tenant response.
/// </summary>
public class TenantDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? LogoUrl { get; set; }
    public string? ThemeColor { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Region { get; set; } = string.Empty;
}
