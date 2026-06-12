using Microsoft.EntityFrameworkCore;
using SalonOS.Identity.Domain;
using SalonOS.Identity.Infrastructure;

namespace SalonOS.Infrastructure.Admin;

/// <summary>
/// The ONE and ONLY place that may call IgnoreQueryFilters() (§R6.4, R5).
/// Every method here is guarded at the controller level by
/// [HasPermission("tenant.manage")] — PlatformOwner only.
///
/// No other file in the codebase may call IgnoreQueryFilters().
/// Task 7.1.
/// </summary>
public sealed class PlatformAdminService
{
    private readonly IdentityDbContext _db;

    public PlatformAdminService(IdentityDbContext db) => _db = db;

    /// <summary>
    /// Returns all tenants (salons) across the platform.
    /// Only callable by PlatformOwner — enforced by [HasPermission("tenant.manage")]
    /// on the controller action that invokes this.
    /// </summary>
    public Task<List<Tenant>> AllTenantsAsync() =>
        _db.Tenants
           .IgnoreQueryFilters()   // SANCTIONED: Tenant is a global entity (no filter), but
                                   // explicit here for future-proofing if filter is added.
           .AsNoTracking()
           .ToListAsync();

    /// <summary>Suspend a tenant (soft-delete).</summary>
    public async Task SuspendTenantAsync(Guid tenantId)
    {
        var tenant = await _db.Tenants.FindAsync(tenantId);
        if (tenant is null) return;
        tenant.IsActive = false;
        await _db.SaveChangesAsync();
    }

    /// <summary>Reactivate a suspended tenant.</summary>
    public async Task ActivateTenantAsync(Guid tenantId)
    {
        var tenant = await _db.Tenants.FindAsync(tenantId);
        if (tenant is null) return;
        tenant.IsActive = true;
        await _db.SaveChangesAsync();
    }
}
