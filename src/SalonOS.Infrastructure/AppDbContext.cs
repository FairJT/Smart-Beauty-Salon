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
    public DbSet<SalonJoinRequest> SalonJoinRequests { get; set; }
    public DbSet<SalonPlacement> SalonPlacements { get; set; }
    public DbSet<BlogPost> BlogPosts { get; set; }
    public DbSet<HomepageSlide> HomepageSlides { get; set; }
    public DbSet<HomepageMenu> HomepageMenus { get; set; }
public DbSet<ClientFeedback> ClientFeedbacks { get; set; }
    public DbSet<ProductUsage> ProductUsages { get; set; }
    public DbSet<RescheduleRequest> RescheduleRequests { get; set; }
    public DbSet<StaffRequest> StaffRequests { get; set; }
    public DbSet<SalonNotice> SalonNotices { get; set; }
    public DbSet<SalonAmenity> SalonAmenities { get; set; }
    public DbSet<ArtistLeave> ArtistLeaves { get; set; }
    public DbSet<ArtistContract> ArtistContracts { get; set; }
    public DbSet<ClientNote> ClientNotes { get; set; }
    public DbSet<WorkingHour> WorkingHours { get; set; }
    public DbSet<SalonClosure> SalonClosures { get; set; }
    public DbSet<StaffServiceContract> StaffServiceContracts { get; set; }
    public DbSet<FinancialTransaction> FinancialTransactions { get; set; }
    public DbSet<Discount> Discounts { get; set; }
    public DbSet<JobPosting> JobPostings { get; set; }
    public DbSet<JobApplication> JobApplications { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ProductUsage>(e => e.Property(p => p.Quantity).HasColumnType("decimal(18,4)"));

        builder.Entity<StaffServiceContract>(e =>
        {
            e.OwnsOne(c => c.Amount);
            e.HasIndex(x => x.ArtistId);
            e.HasIndex(x => x.CatalogServiceId);
        });
        builder.Entity<FinancialTransaction>(e =>
        {
            e.OwnsOne(t => t.Amount);
            e.HasIndex(x => x.CounterpartyUserId);
            e.Property(x => x.CounterpartyUserId).HasMaxLength(450);
        });

        // ── Indexes on FK-like columns + bounded strings (only TenantId is auto-indexed) ──
        builder.Entity<ClientNote>(e =>
        {
            e.HasIndex(x => new { x.ArtistId, x.ClientId });
            e.Property(x => x.ClientId).HasMaxLength(450);
        });
        builder.Entity<ClientFeedback>(e =>
        {
            e.HasIndex(x => x.ClientId);
            e.Property(x => x.ClientId).HasMaxLength(450);
            e.Property(x => x.Title).HasMaxLength(200);
        });
        builder.Entity<StaffRequest>(e =>
        {
            e.HasIndex(x => x.ArtistId);
            e.Property(x => x.Title).HasMaxLength(200);
        });
        builder.Entity<RescheduleRequest>(e =>
        {
            e.HasIndex(x => x.BookingId);
            e.HasIndex(x => x.ArtistId);
        });
        builder.Entity<ProductUsage>(e =>
        {
            e.HasIndex(x => x.BookingId);
            e.HasIndex(x => x.ArtistId);
            e.HasIndex(x => x.InventoryItemId);
        });
        builder.Entity<ArtistContract>(e => e.HasIndex(x => x.ArtistId));
        builder.Entity<ArtistLeave>(e => e.HasIndex(x => x.ArtistId));
        builder.Entity<JobApplication>(e => e.HasIndex(x => x.JobPostingId));
        builder.Entity<Discount>(e =>
        {
            e.HasIndex(x => x.Code);
            e.Property(x => x.Code).HasMaxLength(64);
            e.Property(x => x.TargetClientId).HasMaxLength(450);
        });
        builder.Entity<SalonAmenity>(e => e.Property(x => x.Name).HasMaxLength(100));
        builder.Entity<SalonNotice>(e => e.Property(x => x.Title).HasMaxLength(200));
        builder.Entity<JobPosting>(e => e.Property(x => x.Title).HasMaxLength(200));
        builder.Entity<WorkingHour>(e =>
        {
            e.Property(x => x.OpenTime).HasMaxLength(5);
            e.Property(x => x.CloseTime).HasMaxLength(5);
        });

        builder.Entity<OutboxMessage>(e =>
        {
            e.HasKey(o => o.Id).IsClustered(false);
            e.HasIndex(o => new { o.ProcessedAt, o.CreatedAt }).IsClustered();
        });

        foreach (var et in builder.Model.GetEntityTypes()
                     .Where(t => typeof(TenantEntity).IsAssignableFrom(t.ClrType)))
        {
            builder.Entity(et.ClrType).HasIndex(nameof(TenantEntity.TenantId));

            var p = Expression.Parameter(et.ClrType, "e");
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