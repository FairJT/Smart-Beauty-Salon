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
    public Guid TenantId { get; }
    public bool IsPlatformOwner { get; }

    public TenantContextFromClaims(ICurrentUser currentUser)
    {
        TenantId        = currentUser.TenantId;
        IsPlatformOwner = currentUser.IsPlatformOwner;
    }
}
