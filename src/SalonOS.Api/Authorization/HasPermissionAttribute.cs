using Microsoft.AspNetCore.Authorization;

namespace SalonOS.Api.Authorization;

/// <summary>
/// Shorthand attribute that applies a "perm:{permission}" policy to a controller
/// action. Usage: [HasPermission("appointment.cancel.all")]
/// See §R6.1.
/// </summary>
public sealed class HasPermissionAttribute(string permission)
    : AuthorizeAttribute($"perm:{permission}");
