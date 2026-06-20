namespace SalonOS.Shared;

/// <summary>
/// Interface for tenant context resolution.
/// Resolved by middleware from the authenticated token + active membership.
/// NEVER from a request body/query param.
/// </summary>
public interface ITenantContext
{
    Guid TenantId { get; }
    bool IsPlatformOwner { get; }

    /// <summary>
    /// Public read-only paths (anonymous salon/slots pages) resolve the tenant from a
    /// PUBLIC slug server-side and set it here. Implementations must only apply it when
    /// no tenant is present yet (anonymous); it never overrides an authenticated tenant.
    /// </summary>
    void SetPublicTenant(Guid tenantId);
}
