using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using SalonOS.Shared;

namespace SalonOS.Infrastructure;

public class AppDbContext : DbContext
{
    private readonly ITenantContext _tenant;

    public AppDbContext(DbContextOptions<AppDbContext> options, ITenantContext tenant)
        : base(options)
    {
        _tenant = tenant;
    }

    // ── Outbox ────────────────────────────────────────────────────────────────
    public DbSet<OutboxMessage> OutboxMessages { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        foreach (var et in builder.Model.GetEntityTypes()
                     .Where(t => typeof(TenantEntity).IsAssignableFrom(t.ClrType)))
        {
            builder.Entity(et.ClrType).HasIndex(nameof(TenantEntity.TenantId));

            var p    = Expression.Parameter(et.ClrType, "e");
            var tenantIdProp = Expression.Property(p, nameof(TenantEntity.TenantId));

            var tenantMatch = Expression.Equal(
                tenantIdProp,
                Expression.Property(
                    Expression.Constant(_tenant),
                    nameof(ITenantContext.TenantId)));

            var isPlatformOwner = Expression.Property(
                Expression.Constant(_tenant),
                nameof(ITenantContext.IsPlatformOwner));

            // (e.TenantId == _tenant.TenantId || _tenant.IsPlatformOwner)
            var tenantOrPlatform = Expression.OrElse(tenantMatch, isPlatformOwner);

            // e.IsDeleted == false
            var isDeletedProp = Expression.Property(p, nameof(TenantEntity.IsDeleted));
            var notDeleted = Expression.Not(isDeletedProp);

            // Combine: (e.TenantId == _tenant.TenantId || _tenant.IsPlatformOwner) && !e.IsDeleted
            var body = Expression.AndAlso(tenantOrPlatform, notDeleted);

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
        foreach (var entry in ChangeTracker.Entries<TenantEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.TenantId = _tenant.TenantId;
            }

            if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = DateTime.UtcNow;
            }

            if (entry.State == EntityState.Deleted)
            {
                entry.State = EntityState.Modified;
                entry.Entity.IsDeleted = true;
                entry.Entity.DeletedAt = DateTime.UtcNow;
            }
        }
    }
}
