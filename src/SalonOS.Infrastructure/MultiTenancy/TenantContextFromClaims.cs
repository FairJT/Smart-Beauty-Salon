using SalonOS.Shared;
using SalonOS.Shared.Identity;

namespace SalonOS.Infrastructure.MultiTenancy;

/// <summary>
/// Scoped implementation of ITenantContext.
/// Reads TenantId and IsPlatformOwner from ICurrentUser (which reads from JWT claims).
/// NEVER reads from request body, query string, or headers — R3, R4.
/// See Task 4.1.
/// </summary>
public sealed class TenantContextFromClaims : ITenantContext
{
    public Guid TenantId { get; private set; }
    public bool IsPlatformOwner { get; }

    public TenantContextFromClaims(ICurrentUser currentUser)
    {
        TenantId        = currentUser.TenantId;
        IsPlatformOwner = currentUser.IsPlatformOwner;
    }

    public void SetPublicTenant(Guid tenantId)
    {
        // Only fill an empty (anonymous) tenant; never override an authenticated one,
        // and never for the platform owner.
        if (TenantId == Guid.Empty && !IsPlatformOwner)
            TenantId = tenantId;
    }
}
