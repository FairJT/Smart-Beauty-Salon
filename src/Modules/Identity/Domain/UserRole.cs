namespace SalonOS.Identity.Domain;

/// <summary>
/// Platform-wide user roles for authorization.
/// These are different from MembershipRole which is tenant-specific.
/// </summary>
public enum UserRole
{
    SuperAdmin = 1,
    PlatformAdmin = 2,
    Support = 3,
    User = 4
}
