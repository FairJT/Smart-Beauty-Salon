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
            e.HasIndex(b => b.TenantId);
            e.HasIndex(b => new { b.TenantId, b.Status });
            e.HasIndex(b => new { b.ClientId, b.Status });
            e.HasIndex(b => new { b.ArtistId, b.StartsAt });
            e.HasIndex(b => b.Status);

            e.OwnsOne(b => b.EstimatedPrice);
            e.OwnsOne(b => b.FinalPrice);
            e.OwnsOne(b => b.DepositAmount);
        });

        builder.Entity<ArtistSchedule>(e =>
        {
            e.HasIndex(s => new { s.TenantId, s.ArtistId });
            e.HasIndex(s => new { s.ArtistId, s.DayOfWeek }).IsUnique()
                .HasFilter("[IsDeleted] = 0");
        });

        builder.Entity<Leave>(e =>
        {
            e.HasIndex(l => new { l.TenantId, l.ArtistId });
            e.HasIndex(l => new { l.ArtistId, l.StartDateTime, l.EndDateTime });
        });
    }
}
