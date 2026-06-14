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
}
