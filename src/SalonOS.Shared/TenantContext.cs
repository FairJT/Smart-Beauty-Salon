namespace SalonOS.Shared;

/// <summary>
/// Default implementation of ITenantContext.
/// Used in scoped DI to provide tenant information per request.
/// </summary>
public class TenantContext : ITenantContext
{
    public Guid TenantId { get; set; }
    public bool IsPlatformOwner { get; set; }

    public void SetPublicTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty && !IsPlatformOwner)
            TenantId = tenantId;
    }
}
