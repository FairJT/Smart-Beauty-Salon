using System.ComponentModel.DataAnnotations;

namespace SalonOS.Marketplace.Application.DTOs;

/// <summary>
/// DTO for creating a service template.
/// </summary>
public class CreateServiceTemplateDto
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [Required]
    [MaxLength(100)]
    public string Category { get; set; } = string.Empty;
}

/// <summary>
/// DTO for updating a service template.
/// </summary>
public class UpdateServiceTemplateDto
{
    [MaxLength(100)]
    public string? Name { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    [MaxLength(100)]
    public string? Category { get; set; }
}

/// <summary>
/// DTO for service template response.
/// </summary>
public class ServiceTemplateDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Category { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<TemplateOptionGroupDto> OptionGroups { get; set; } = new();
}

/// <summary>
/// DTO for template option group.
/// </summary>
public class TemplateOptionGroupDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsRequired { get; set; }
    public int SortOrder { get; set; }
    public List<TemplateOptionDto> Options { get; set; } = new();
}

/// <summary>
/// DTO for template option.
/// </summary>
public class TemplateOptionDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}
