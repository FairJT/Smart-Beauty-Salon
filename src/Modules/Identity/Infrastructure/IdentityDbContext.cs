using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SalonOS.Identity.Domain;
using SalonOS.Shared;

namespace SalonOS.Identity.Infrastructure;

/// <summary>
/// Identity database context for ASP.NET Identity.
/// This context handles user and role management.
/// </summary>
public class IdentityDbContext : IdentityDbContext<ApplicationUser>
{
    public IdentityDbContext(DbContextOptions<IdentityDbContext> options)
        : base(options)
    {
    }

    public DbSet<Tenant> Tenants { get; set; }
    public DbSet<Membership> Memberships { get; set; }
    public DbSet<SalonManagerProfile> SalonManagerProfiles { get; set; }
    public DbSet<ArtistProfile> ArtistProfiles { get; set; }
    public DbSet<ClientProfile> ClientProfiles { get; set; }
    public DbSet<JobSeekerProfile> JobSeekerProfiles { get; set; }
    public DbSet<SavedSalon> SavedSalons { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Tenant configuration
        builder.Entity<Tenant>(e =>
        {
            e.HasIndex(t => t.Slug).IsUnique();
            e.HasIndex(t => t.IsActive);
            e.HasIndex(t => t.SalonId).IsUnique();
            e.Property(t => t.SalonId).ValueGeneratedOnAdd();
        });

        // Membership configuration
        builder.Entity<Membership>(e =>
        {
            e.HasKey(m => m.Id).IsClustered(false);
            e.HasIndex(m => new { m.TenantId, m.UserId }).IsClustered().IsUnique();

            e.HasIndex(m => m.TenantId);

            e.HasOne(m => m.User)
                .WithMany()
                .HasForeignKey(m => m.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(m => m.Tenant)
                .WithMany()
                .HasForeignKey(m => m.TenantId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // SalonManagerProfile configuration
        builder.Entity<SalonManagerProfile>(e =>
        {
            e.HasIndex(p => p.UserId).IsUnique();
            e.HasIndex(p => p.TenantId);

            e.HasOne(p => p.User)
                .WithMany()
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(p => p.Tenant)
                .WithMany()
                .HasForeignKey(p => p.TenantId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ArtistProfile configuration
        builder.Entity<ArtistProfile>(e =>
        {
            e.HasIndex(p => p.UserId).IsUnique();
            e.HasIndex(p => p.TenantId);

            e.HasOne(p => p.User)
                .WithMany()
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(p => p.Tenant)
                .WithMany()
                .HasForeignKey(p => p.TenantId)
                .OnDelete(DeleteBehavior.Cascade);

            e.OwnsOne(p => p.Salary, m =>
            {
                m.Property(x => x.Amount).HasColumnName("SalaryAmount");
                m.Property(x => x.Currency).HasColumnName("SalaryCurrency").HasMaxLength(3);
            });

            e.OwnsOne(p => p.RentAmount, m =>
            {
                m.Property(x => x.Amount).HasColumnName("RentAmount");
                m.Property(x => x.Currency).HasColumnName("RentCurrency").HasMaxLength(3);
            });
        });

        // ClientProfile configuration
        builder.Entity<ClientProfile>(e =>
        {
            e.HasIndex(p => p.UserId).IsUnique();

            e.HasOne(p => p.User)
                .WithMany()
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // JobSeekerProfile configuration
        builder.Entity<JobSeekerProfile>(e =>
        {
            e.HasIndex(p => p.UserId).IsUnique();

            e.HasOne(p => p.User)
                .WithMany()
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // SavedSalon configuration — user-owned, NOT tenant-scoped
        // Client can favorite salons across multiple tenants (A-06)
        builder.Entity<SavedSalon>(e =>
        {
            e.HasKey(s => s.Id).IsClustered(false);
            e.HasIndex(s => new { s.UserId, s.Slug }).IsClustered().IsUnique();
        });
    }
}
