using Microsoft.EntityFrameworkCore;
using SalonOS.Booking.Domain;

namespace SalonOS.Booking.Infrastructure;

/// <summary>
/// Booking database context.
/// Handles persistence for booking entities.
/// </summary>
public class BookingDbContext : DbContext
{
    public BookingDbContext(DbContextOptions<BookingDbContext> options)
        : base(options)
    {
    }

    public DbSet<SalonOS.Booking.Domain.Booking> Bookings { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

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
    }
}
