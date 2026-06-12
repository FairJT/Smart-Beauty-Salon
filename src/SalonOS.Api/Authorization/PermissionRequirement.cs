using Microsoft.AspNetCore.Authorization;

namespace SalonOS.Api.Authorization;

/// <summary>
/// Represents the requirement that a user holds a specific permission claim.
/// See §R6.1 — permission-based policies.
/// </summary>
public sealed class PermissionRequirement(string permission) : IAuthorizationRequirement
{
    public string Permission { get; } = permission;
}
