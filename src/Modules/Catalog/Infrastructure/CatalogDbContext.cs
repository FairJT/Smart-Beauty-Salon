using Microsoft.EntityFrameworkCore;
using SalonOS.Catalog.Domain;

namespace SalonOS.Catalog.Infrastructure;

/// <summary>
/// Catalog database context.
/// Handles persistence for catalog entities.
/// </summary>
public class CatalogDbContext : DbContext
{
    public CatalogDbContext(DbContextOptions<CatalogDbContext> options)
        : base(options)
    {
    }

    public DbSet<CatalogService> CatalogServices { get; set; }
    public DbSet<CatalogServiceOption> CatalogServiceOptions { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // CatalogService configuration (TENANT entity)
        builder.Entity<CatalogService>(e =>
        {
            e.HasIndex(s => s.TenantId);
            e.HasIndex(s => new { s.TenantId, s.IsActive });
            e.HasIndex(s => s.Category);

            e.OwnsOne(s => s.BasePrice);
        });

        // CatalogServiceOption configuration (TENANT entity)
        builder.Entity<CatalogServiceOption>(e =>
        {
            e.HasIndex(o => o.TenantId);
            e.HasIndex(o => new { o.CatalogServiceId, o.IsActive });

            e.HasOne(o => o.CatalogService)
                .WithMany(s => s.Options)
                .HasForeignKey(o => o.CatalogServiceId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
