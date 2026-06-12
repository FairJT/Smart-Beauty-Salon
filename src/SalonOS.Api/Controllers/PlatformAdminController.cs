using Microsoft.AspNetCore.Mvc;
using SalonOS.Api.Authorization;
using SalonOS.Infrastructure.Admin;
using SalonOS.Shared.Authorization;

namespace SalonOS.Api.Controllers;

/// <summary>
/// Platform administration controller — PlatformOwner only.
/// Every action requires tenant.manage, which only PlatformOwner holds.
/// Delegates cross-tenant reads to PlatformAdminService (the ONLY sanctioned
/// caller of IgnoreQueryFilters — §R6.4, R5).
/// Task 7.1.
/// </summary>
[Route("api/admin")]
[ApiController]
[HasPermission(Permissions.TenantManage)]
public class PlatformAdminController : ControllerBase
{
    private readonly PlatformAdminService _admin;

    public PlatformAdminController(PlatformAdminService admin) => _admin = admin;

    /// <summary>List all tenants across the platform.</summary>
    [HttpGet("tenants")]
    public async Task<IActionResult> GetAllTenants()
    {
        var tenants = await _admin.AllTenantsAsync();
        return Ok(tenants);
    }

    /// <summary>Suspend a tenant.</summary>
    [HttpPost("tenants/{id}/suspend")]
    public async Task<IActionResult> SuspendTenant(Guid id)
    {
        await _admin.SuspendTenantAsync(id);
        return Ok(new { message = "Tenant suspended" });
    }

    /// <summary>Reactivate a tenant.</summary>
    [HttpPost("tenants/{id}/activate")]
    public async Task<IActionResult> ActivateTenant(Guid id)
    {
        await _admin.ActivateTenantAsync(id);
        return Ok(new { message = "Tenant activated" });
    }
}
