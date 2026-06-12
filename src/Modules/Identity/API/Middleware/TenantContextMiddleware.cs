using Microsoft.AspNetCore.Http;

namespace SalonOS.Identity.API.Middleware;

/// <summary>
/// Legacy middleware — kept as a pass-through so existing registrations don't break.
/// Tenant resolution is now handled entirely by TenantContextFromClaims (Task 4.1),
/// which reads from JWT claims via ICurrentUser.
///
/// The old X-Tenant-Id header path has been removed (violated R3).
/// The security net that rejects protected endpoints without a tenant
/// is now in TenantResolutionMiddleware (Task 4.5).
/// </summary>
public class TenantContextMiddleware
{
    private readonly RequestDelegate _next;

    public TenantContextMiddleware(RequestDelegate next) => _next = next;

    public Task InvokeAsync(HttpContext context) => _next(context);
}
