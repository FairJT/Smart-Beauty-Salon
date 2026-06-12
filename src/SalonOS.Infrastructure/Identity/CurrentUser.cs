using Microsoft.AspNetCore.Http;
using SalonOS.Shared.Identity;
using System.Security.Claims;

namespace SalonOS.Infrastructure.Identity;

/// <summary>
/// Reads the current user's identity from the HTTP context claims.
/// All values originate from the validated JWT — never from request body or query params.
/// Registered as Scoped so each request gets a fresh instance. See Task 3.1.
/// </summary>
public sealed class CurrentUser : ICurrentUser
{
    public string UserId { get; }
    public Guid? ArtistId { get; }
    public string Role { get; }
    public Guid TenantId { get; }
    public bool IsPlatformOwner { get; }

    public CurrentUser(IHttpContextAccessor accessor)
    {
        var user = accessor.HttpContext?.User;

        UserId = user?.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

        Role = user?.FindFirstValue("role") ?? string.Empty;

        IsPlatformOwner = string.Equals(
            user?.FindFirstValue("is_platform_owner"), "true",
            StringComparison.OrdinalIgnoreCase);

        var tenantClaim = user?.FindFirstValue("tenant_id");
        TenantId = Guid.TryParse(tenantClaim, out var tid) ? tid : Guid.Empty;

        var artistClaim = user?.FindFirstValue("artist_id");
        ArtistId = Guid.TryParse(artistClaim, out var aid) ? aid : null;
    }
}
