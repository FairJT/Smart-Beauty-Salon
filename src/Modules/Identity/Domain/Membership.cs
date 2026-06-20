namespace SalonOS.Identity.Domain;

/// <summary>
/// Membership entity - links users to tenants with roles.
/// This is a GLOBAL entity (no TenantId) as it defines the relationship between users and tenants.
/// </summary>
public class Membership
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string UserId { get; set; } = string.Empty;
    public Guid TenantId { get; set; }
    public MembershipRole Role { get; set; } = MembershipRole.Member;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public ApplicationUser User { get; set; } = null!;
    public Tenant Tenant { get; set; } = null!;
}

/// <summary>
/// Membership role within a tenant.
/// </summary>
public enum MembershipRole
{
    Owner        = 1,
    Admin        = 2,
    Manager      = 3,
    Receptionist = 4,   // DEPRECATED — folded into SalonManager (book-on-behalf). Do not assign.
    Staff        = 5,   // Artist / stylist
    Member       = 6    // Client
}
