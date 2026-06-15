using Microsoft.EntityFrameworkCore;
using SalonOS.Marketplace.Domain;

namespace SalonOS.Marketplace.Infrastructure;

/// <summary>
/// Marketplace database context.
/// Handles persistence for marketplace entities.
/// </summary>
public class MarketplaceDbContext : DbContext
{
    public MarketplaceDbContext(DbContextOptions<MarketplaceDbContext> options)
        : base(options)
    {
    }

    public DbSet<ServiceTemplate> ServiceTemplates { get; set; }
    public DbSet<TemplateOptionGroup> TemplateOptionGroups { get; set; }
    public DbSet<TemplateOption> TemplateOptions { get; set; }
    public DbSet<PackageListing> PackageListings { get; set; }
    public DbSet<SalonPackageLicense> SalonPackageLicenses { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // ServiceTemplate configuration (GLOBAL entity)
        builder.Entity<ServiceTemplate>(e =>
        {
            e.HasIndex(t => t.Name);
            e.HasIndex(t => t.Category);
            e.HasIndex(t => t.IsActive);
        });

        // TemplateOptionGroup configuration (GLOBAL entity)
        builder.Entity<TemplateOptionGroup>(e =>
        {
            e.HasIndex(g => g.ServiceTemplateId);
            e.HasIndex(g => g.IsActive);

            e.HasOne(g => g.ServiceTemplate)
                .WithMany(t => t.OptionGroups)
                .HasForeignKey(g => g.ServiceTemplateId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // TemplateOption configuration (GLOBAL entity)
        builder.Entity<TemplateOption>(e =>
        {
            e.HasIndex(o => o.OptionGroupId);
            e.HasIndex(o => o.IsActive);

            e.HasOne(o => o.OptionGroup)
                .WithMany(g => g.Options)
                .HasForeignKey(o => o.OptionGroupId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // PackageListing configuration (GLOBAL entity)
        builder.Entity<PackageListing>(e =>
        {
            e.OwnsOne(p => p.Price);
            e.HasIndex(p => p.Category);
            e.HasIndex(p => p.IsActive);
        });

        // SalonPackageLicense configuration (TENANT entity)
        builder.Entity<SalonPackageLicense>(e =>
        {
            e.HasQueryFilter(l => !l.IsDeleted);
            e.HasIndex(l => l.TenantId);
            e.HasIndex(l => l.PackageListingId);
            e.HasIndex(l => l.IsActive);

            e.OwnsOne(l => l.PaidAmount);

            e.HasOne(l => l.PackageListing)
                .WithMany()
                .HasForeignKey(l => l.PackageListingId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
