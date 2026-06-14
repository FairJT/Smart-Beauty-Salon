namespace SalonOS.Identity.Domain;

public class SalonManagerProfile
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string UserId { get; set; } = string.Empty;
    public Guid TenantId { get; set; }
    public int? SalonId { get; set; }
    public bool IsOwner { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ApplicationUser User { get; set; } = null!;
    public Tenant Tenant { get; set; } = null!;
}
