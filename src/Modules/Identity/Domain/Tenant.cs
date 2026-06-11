namespace SalonOS.Identity.Domain;

/// <summary>
/// Tenant entity - represents a salon or business.
/// This is a GLOBAL entity (no TenantId) as tenants are the root of multi-tenancy.
/// </summary>
public class Tenant
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? LogoUrl { get; set; }
    public string? ThemeColor { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string Region { get; set; } = "IR"; // For payment provider selection
}
