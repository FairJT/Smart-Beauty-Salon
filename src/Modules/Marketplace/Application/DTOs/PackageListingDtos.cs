using System.ComponentModel.DataAnnotations;
using SalonOS.Shared;

namespace SalonOS.Marketplace.Application.DTOs;

/// <summary>
/// DTO for creating a package listing.
/// </summary>
public class CreatePackageListingDto
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
    public long PriceAmount { get; set; }

    [Required]
    [MaxLength(3)]
    public string PriceCurrency { get; set; } = "IRR";

    [Required]
    [Range(1, 120)]
    public int DurationMonths { get; set; }
}

/// <summary>
/// DTO for package listing response.
/// </summary>
public class PackageListingDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Category { get; set; } = string.Empty;
    public Money Price { get; set; }
    public int DurationMonths { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// DTO for salon package license response.
/// </summary>
public class SalonPackageLicenseDto
{
    public Guid Id { get; set; }
    public Guid PackageListingId { get; set; }
    public string? PackageName { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; }
    public Money PaidAmount { get; set; }
    public DateTime CreatedAt { get; set; }
}
