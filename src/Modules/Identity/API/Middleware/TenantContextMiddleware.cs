using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using SalonOS.Shared;

namespace SalonOS.Identity.API.Middleware;

/// <summary>
/// Middleware to resolve tenant context from JWT token.
/// Sets the ITenantContext for the current request.
/// 
/// Flow:
/// 1. User logs in -> gets JWT without TenantId
/// 2. User selects active tenant -> calls /api/auth/select-tenant endpoint
/// 3. Backend adds TenantId to JWT claims (or uses a separate mechanism)
/// 4. For subsequent requests, this middleware extracts TenantId from JWT
/// 
/// For now, we'll use a simple approach: TenantId comes from a custom header or claim.
/// </summary>
public class TenantContextMiddleware
{
    private readonly RequestDelegate _next;

    public TenantContextMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var tenantContext = context.RequestServices.GetRequiredService<ITenantContext>() as TenantContext;
        
        if (tenantContext != null)
        {
            // Try to get TenantId from JWT claims
            var tenantIdClaim = context.User?.FindFirst("TenantId");
            if (tenantIdClaim != null && Guid.TryParse(tenantIdClaim.Value, out var tenantId))
            {
                tenantContext.TenantId = tenantId;
            }
            else
            {
                // Fallback: Try to get TenantId from X-Tenant-Id header
                // This is used when the client sends the active tenant ID
                if (context.Request.Headers.TryGetValue("X-Tenant-Id", out var headerTenantId))
                {
                    if (Guid.TryParse(headerTenantId, out var headerTenantIdValue))
                    {
                        tenantContext.TenantId = headerTenantIdValue;
                    }
                }
            }

            // Check if user is platform owner
            var userTypeClaim = context.User?.FindFirst("UserType");
            if (userTypeClaim?.Value == "SuperAdmin")
            {
                tenantContext.IsPlatformOwner = true;
            }
        }

        await _next(context);
    }
}
