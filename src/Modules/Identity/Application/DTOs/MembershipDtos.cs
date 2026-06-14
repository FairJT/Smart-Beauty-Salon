using System.ComponentModel.DataAnnotations;
using SalonOS.Identity.Domain;

namespace SalonOS.Identity.Application.DTOs;

/// <summary>
/// DTO for creating a membership.
/// </summary>
public class CreateMembershipDto
{
    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required]
    public Guid TenantId { get; set; }

    [Required]
    public MembershipRole Role { get; set; } = MembershipRole.Member;
}

/// <summary>
/// DTO for membership response.
/// </summary>
public class MembershipDto
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public Guid TenantId { get; set; }
    public MembershipRole Role { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    
    // Navigation properties
    public string? UserName { get; set; }
    public string? TenantName { get; set; }
}
