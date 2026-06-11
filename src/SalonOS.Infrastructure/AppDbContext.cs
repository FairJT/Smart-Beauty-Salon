using Microsoft.EntityFrameworkCore;
using SalonOS.Shared;

namespace SalonOS.Infrastructure;

/// <summary>
/// Main database context for SalonOS.
/// Implements global query filters for multi-tenancy.
/// Will inherit from IdentityDbContext once Identity module is created.
/// </summary>
public class AppDbContext : DbContext
{
    private readonly ITenantContext _tenant;

    public AppDbContext(DbContextOptions<AppDbContext> options, ITenantContext tenant)
        : base(options)
    {
        _tenant = tenant;
    }

    // Domain entities will be registered here as we create them
    // public DbSet<Salon> Salons { get; set; }
    // public DbSet<Artist> Artists { get; set; }
    // etc.

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Apply global query filters for tenant-owned entities
        // This will be implemented as we create tenant entities
        // foreach (var entityType in builder.Model.GetEntityTypes())
        // {
        //     if (typeof(TenantEntity).IsAssignableFrom(entityType.ClrType))
        //     {
        //         var method = typeof(AppDbContext)
        //             .GetMethod(nameof(ApplyTenantFilter), BindingFlags.NonPublic | BindingFlags.Static)!
        //             .MakeGenericMethod(entityType.ClrType);
        //         method.Invoke(null, new object[] { builder });
        //     }
        // }
    }

    public override int SaveChanges()
    {
        StampTenant();
        return base.SaveChanges();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        StampTenant();
        return await base.SaveChangesAsync(cancellationToken);
    }

    private void StampTenant()
    {
        foreach (var entry in ChangeTracker.Entries<TenantEntity>()
                     .Where(e => e.State == EntityState.Added))
        {
            entry.Entity.TenantId = _tenant.TenantId;
        }
    }

    // private static void ApplyTenantFilter<T>(ModelBuilder builder) where T : TenantEntity
    // {
    //     builder.Entity<T>().HasQueryFilter(e => e.TenantId == currentTenantId);
    // }
}
