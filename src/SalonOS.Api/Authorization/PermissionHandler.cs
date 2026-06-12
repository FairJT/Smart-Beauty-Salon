using Microsoft.AspNetCore.Authorization;

namespace SalonOS.Api.Authorization;

/// <summary>
/// Grants the requirement when the user holds a "permission" claim matching
/// the required permission string. See §R6.1.
/// </summary>
public sealed class PermissionHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        if (context.User.HasClaim("permission", requirement.Permission))
            context.Succeed(requirement);

        return Task.CompletedTask;
    }
}
