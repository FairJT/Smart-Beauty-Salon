using Microsoft.AspNetCore.Authorization;

namespace SalonOS.Shared.Authorization;

public sealed class PermissionRequirement(string permission) : IAuthorizationRequirement
{
    public string Permission { get; } = permission;
}
