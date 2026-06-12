using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using SalonOS.Shared;

namespace SalonOS.Infrastructure;

/// <summary>
/// Main application DbContext.
///
/// Task 4.3: global query filter — every TenantEntity query is automatically
/// scoped to _tenant.TenantId, so services never need a manual .Where(e => e.TenantId == …).
/// PlatformOwner bypasses the filter: when IsPlatformOwner is true the predicate
/// evaluates to true for all rows. The ONLY sanctioned cross-tenant override is in
/// PlatformAdminService.IgnoreQueryFilters() (Task 7.1).
///
/// Task 4.4: SaveChanges stamps TenantId on new entities from context, never from
/// the incoming DTO.
/// </summary>
public class AppDbContext : DbContext
{
    private readonly ITenantContext _tenant;

    public AppDbContext(DbContextOptions<AppDbContext> options, ITenantContext tenant)
        : base(options)
    {
        _tenant = tenant;
    }

    // ── DbSets (add as modules introduce entities) ────────────────────────────
    // public DbSet<Booking> Bookings { get; set; }
    // etc. — module DbContexts own their own sets; this context owns shared infra.

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // ── Global query filter (§R6.3) ──────────────────────────────────────
        // For every entity that derives from TenantEntity, add a filter:
        //   e.TenantId == _tenant.TenantId  OR  IsPlatformOwner == true
        // This runs as a compiled expression so EF can translate it to SQL.
        foreach (var et in builder.Model.GetEntityTypes()
                     .Where(t => typeof(TenantEntity).IsAssignableFrom(t.ClrType)))
        {
            builder.Entity(et.ClrType).HasIndex(nameof(TenantEntity.TenantId));

            var p    = Expression.Parameter(et.ClrType, "e");
            var tenantIdProp = Expression.Property(p, nameof(TenantEntity.TenantId));

            // e.TenantId == _tenant.TenantId
            var tenantMatch = Expression.Equal(
                tenantIdProp,
                Expression.Property(
                    Expression.Constant(_tenant),
                    nameof(ITenantContext.TenantId)));

            // _tenant.IsPlatformOwner  (bypasses filter for PlatformOwner)
            var isPlatformOwner = Expression.Property(
                Expression.Constant(_tenant),
                nameof(ITenantContext.IsPlatformOwner));

            // e.TenantId == _tenant.TenantId || _tenant.IsPlatformOwner
            var body = Expression.OrElse(tenantMatch, isPlatformOwner);

            builder.Entity(et.ClrType).HasQueryFilter(Expression.Lambda(body, p));
        }
    }

    // ── Task 4.4: stamp TenantId from context on every new entity ────────────
    public override int SaveChanges()
    {
        StampTenant();
        return base.SaveChanges();
    }

    public override async Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        StampTenant();
        return await base.SaveChangesAsync(cancellationToken);
    }

    private void StampTenant()
    {
        foreach (var entry in ChangeTracker.Entries<TenantEntity>()
                     .Where(e => e.State == EntityState.Added))
        {
            // Always overwrite — even if the DTO set it, the context value wins (R4).
            entry.Entity.TenantId = _tenant.TenantId;
        }
    }
}
