namespace SalonOS.Shared;

/// <summary>
/// Base class for all tenant-owned entities.
/// Every tenant-owned entity must derive from this class.
/// Global entities (User, Tenant, ServiceTemplate, etc.) do NOT derive from this.
/// </summary>
public abstract class TenantEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
}
