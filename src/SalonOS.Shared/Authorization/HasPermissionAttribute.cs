using Microsoft.AspNetCore.Authorization;

namespace SalonOS.Shared.Authorization;

public sealed class HasPermissionAttribute(string permission)
    : AuthorizeAttribute($"perm:{permission}");
