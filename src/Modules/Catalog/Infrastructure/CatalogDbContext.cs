using Microsoft.EntityFrameworkCore;
using SalonOS.Catalog.Domain;

namespace SalonOS.Catalog.Infrastructure;

public class CatalogDbContext : DbContext
{
    public CatalogDbContext(DbContextOptions<CatalogDbContext> options)
        : base(options)
    {
    }

    public DbSet<ServiceType> ServiceTypes { get; set; }
    public DbSet<CatalogService> CatalogServices { get; set; }
    public DbSet<ServiceOption> ServiceOptions { get; set; }
    public DbSet<Material> Materials { get; set; }
    public DbSet<SalonMedia> SalonMedia { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // ServiceType configuration (GLOBAL entity — no TenantId)
        builder.Entity<ServiceType>(e =>
        {
            e.HasIndex(t => t.Name).IsUnique();
            e.HasIndex(t => t.Category);
            e.HasIndex(t => t.IsActive);
        });

        // CatalogService configuration (TENANT entity)
        builder.Entity<CatalogService>(e =>
        {
            e.HasQueryFilter(s => !s.IsDeleted);
            e.HasIndex(s => s.TenantId);
            e.HasIndex(s => new { s.TenantId, s.IsActive });
            e.HasIndex(s => s.ServiceTypeId);

            e.OwnsOne(s => s.BasePrice, b =>
            {
                b.Property(m => m.Amount).HasColumnName("BasePrice_Amount");
                b.Property(m => m.Currency).HasColumnName("BasePrice_Currency").HasMaxLength(3);
            });

            e.HasOne(s => s.ServiceType)
                .WithMany()
                .HasForeignKey(s => s.ServiceTypeId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ServiceOption configuration (TENANT entity)
        builder.Entity<ServiceOption>(e =>
        {
            e.HasQueryFilter(o => !o.IsDeleted);
            e.HasIndex(o => o.TenantId);
            e.HasIndex(o => new { o.CatalogServiceId, o.IsActive });

            e.OwnsOne(o => o.PriceDelta, b =>
            {
                b.Property(m => m.Amount).HasColumnName("PriceDelta_Amount");
                b.Property(m => m.Currency).HasColumnName("PriceDelta_Currency").HasMaxLength(3);
            });

            e.HasOne(o => o.CatalogService)
                .WithMany(s => s.Options)
                .HasForeignKey(o => o.CatalogServiceId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Material configuration (TENANT entity)
        builder.Entity<Material>(e =>
        {
            e.HasQueryFilter(m => !m.IsDeleted);
            e.HasIndex(m => m.TenantId);
            e.HasIndex(m => m.Name);

            e.OwnsOne(m => m.Price, b =>
            {
                b.Property(p => p.Amount).HasColumnName("Price_Amount");
                b.Property(p => p.Currency).HasColumnName("Price_Currency").HasMaxLength(3);
            });

            e.HasMany(m => m.SalonServices)
                .WithMany(s => s.Materials)
                .UsingEntity(j => j.ToTable("CatalogServiceMaterials"));
        });

        // SalonMedia configuration (TENANT entity)
        builder.Entity<SalonMedia>(e =>
        {
            e.HasQueryFilter(m => !m.IsDeleted);
            e.HasIndex(m => m.TenantId);
            e.HasIndex(m => new { m.SalonId, m.SortOrder });
            e.HasIndex(m => m.MediaType);
        });
    }
}
