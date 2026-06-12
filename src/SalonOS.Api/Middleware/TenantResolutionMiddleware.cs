using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using SalonOS.Shared;
using SalonOS.Shared.Identity;

namespace SalonOS.Api.Middleware;

/// <summary>
/// Safety-net middleware that runs AFTER UseAuthentication, BEFORE UseAuthorization.
///
/// For every authenticated request to a protected endpoint:
///   - If the user is PlatformOwner  → allow (they cross tenants by design).
///   - If TenantId is empty/zero     → 401 Unauthorized.
///
/// Tenant id always comes from the validated JWT claim (via ICurrentUser / ITenantContext).
/// It is NEVER read from request body, query string, or headers (R3).
///
/// See Task 4.5.
/// </summary>
public sealed class TenantResolutionMiddleware
{
    private readonly RequestDelegate _next;

    public TenantResolutionMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        // Only inspect authenticated requests
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var currentUser   = context.RequestServices.GetRequiredService<ICurrentUser>();
            var tenantContext = context.RequestServices.GetRequiredService<ITenantContext>();

            // PlatformOwner crosses tenants — always allow
            if (!currentUser.IsPlatformOwner)
            {
                // Protected endpoint without a valid tenant claim → reject
                if (tenantContext.TenantId == Guid.Empty)
                {
                    // Check whether the endpoint explicitly allows anonymous (login, register, etc.)
                    var endpoint = context.GetEndpoint();
                    var allowAnon = endpoint?.Metadata.GetMetadata<IAllowAnonymous>() != null;

                    if (!allowAnon)
                    {
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        await context.Response.WriteAsync(
                            "No valid tenant in token. Please log in under an active salon membership.");
                        return;
                    }
                }
            }
        }

        await _next(context);
    }
}
