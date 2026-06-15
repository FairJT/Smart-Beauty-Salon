using Microsoft.EntityFrameworkCore;
using SalonOS.Booking.Domain;
using SalonOS.Shared;

namespace SalonOS.Booking.Infrastructure;

public class BookingDbContext : DbContext
{
    public BookingDbContext(DbContextOptions<BookingDbContext> options)
        : base(options)
    {
    }

    public DbSet<SalonOS.Booking.Domain.Booking> Bookings { get; set; }
    public DbSet<ArtistSchedule> ArtistSchedules { get; set; }
    public DbSet<Leave> Leaves { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Ignore<DomainEvent>();

        builder.Entity<SalonOS.Booking.Domain.Booking>(e =>
        {
            e.HasQueryFilter(b => !b.IsDeleted);

            e.HasKey(b => b.Id).IsClustered(false);
            e.HasIndex(b => new { b.TenantId, b.CreatedAt }).IsClustered().IsDescending(false, true);

            e.HasIndex(b => b.TenantId);
            e.HasIndex(b => new { b.TenantId, b.Status });
            e.HasIndex(b => new { b.ClientId, b.Status });
            e.HasIndex(b => new { b.ArtistId, b.StartsAt });
            e.HasIndex(b => b.Status);

            e.OwnsOne(b => b.EstimatedPrice, b1 =>
            {
                b1.Property(m => m.Amount).HasColumnName("EstimatedPrice_Amount");
                b1.Property(m => m.Currency).HasColumnName("EstimatedPrice_Currency").HasMaxLength(3);
            });
            e.OwnsOne(b => b.FinalPrice, b1 =>
            {
                b1.Property(m => m.Amount).HasColumnName("FinalPrice_Amount");
                b1.Property(m => m.Currency).HasColumnName("FinalPrice_Currency").HasMaxLength(3);
            });
            e.OwnsOne(b => b.DepositAmount, b1 =>
            {
                b1.Property(m => m.Amount).HasColumnName("DepositAmount_Amount");
                b1.Property(m => m.Currency).HasColumnName("DepositAmount_Currency").HasMaxLength(3);
            });
        });

        builder.Entity<ArtistSchedule>(e =>
        {
            e.HasQueryFilter(s => !s.IsDeleted);

            e.HasIndex(s => new { s.TenantId, s.ArtistId });
            e.HasIndex(s => new { s.ArtistId, s.DayOfWeek }).IsUnique()
                .HasFilter("[IsDeleted] = 0");
        });

        builder.Entity<Leave>(e =>
        {
            e.HasQueryFilter(l => !l.IsDeleted);

            e.HasIndex(l => new { l.TenantId, l.ArtistId });
            e.HasIndex(l => new { l.ArtistId, l.StartDateTime, l.EndDateTime });
        });
    }
}
