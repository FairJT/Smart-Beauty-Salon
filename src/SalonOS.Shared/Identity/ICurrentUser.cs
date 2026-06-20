namespace SalonOS.Shared.Identity;

/// <summary>
/// Represents the currently authenticated user for the request.
/// All values come from validated JWT claims — never from request input.
/// See Task 3.1.
/// </summary>
public interface ICurrentUser
{
    /// <summary>ASP.NET Identity user id (string guid).</summary>
    string UserId { get; }

    /// <summary>Artist profile id — non-null only when Role == "Artist".</summary>
    Guid? ArtistId { get; }

    /// <summary>
    /// The role name as stored in the token: SalonManager, Artist,
    /// Client, or PlatformOwner.
    /// </summary>
    string Role { get; }

    /// <summary>
    /// The tenant the user is acting in.
    /// Empty guid for PlatformOwner (they cross tenants via PlatformAdminService).
    /// </summary>
    Guid TenantId { get; }

    /// <summary>True when the user is the PlatformOwner (SuperAdmin).</summary>
    bool IsPlatformOwner { get; }
}
