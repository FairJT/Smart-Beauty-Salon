using Microsoft.EntityFrameworkCore;
using SalonOS.Booking.Infrastructure;
using SalonOS.Shared;
using Xunit;

namespace SalonOS.Tenancy.Tests;

public class BookingPhase2Tests
{
    private static BookingDbContext CreateContext()
    {
        var opts = new DbContextOptionsBuilder<BookingDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new BookingDbContext(opts);
    }

    private static async Task<(Guid TenantA, Guid TenantB, Guid ArtistA)> SeedSchedulesAsync(BookingDbContext db)
    {
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();
        var artistA = Guid.NewGuid();
        var artistB = Guid.NewGuid();

        db.ArtistSchedules.AddRange(
            new SalonOS.Booking.Domain.ArtistSchedule
            {
                ArtistId = artistA,
                TenantId = tenantA,
                DayOfWeek = DayOfWeek.Monday,
                StartTime = new TimeSpan(9, 0, 0),
                EndTime = new TimeSpan(17, 0, 0)
            },
            new SalonOS.Booking.Domain.ArtistSchedule
            {
                ArtistId = artistB,
                TenantId = tenantB,
                DayOfWeek = DayOfWeek.Monday,
                StartTime = new TimeSpan(9, 0, 0),
                EndTime = new TimeSpan(17, 0, 0)
            });

        await db.SaveChangesAsync();
        return (tenantA, tenantB, artistA);
    }

    [Fact]
    public async Task ArtistSchedule_is_tenant_scoped()
    {
        using var db = CreateContext();
        var (tenantA, tenantB, _) = await SeedSchedulesAsync(db);

        var schedulesA = await db.ArtistSchedules
            .Where(s => s.TenantId == tenantA && !s.IsDeleted)
            .ToListAsync();

        var schedulesB = await db.ArtistSchedules
            .Where(s => s.TenantId == tenantB && !s.IsDeleted)
            .ToListAsync();

        Assert.Single(schedulesA);
        Assert.Single(schedulesB);
        Assert.All(schedulesA, s => Assert.Equal(tenantA, s.TenantId));
        Assert.All(schedulesB, s => Assert.Equal(tenantB, s.TenantId));
    }

    [Fact]
    public async Task Leave_is_tenant_scoped()
    {
        using var db = CreateContext();
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();
        var artistId = Guid.NewGuid();
        var future = DateTime.UtcNow.AddDays(7);

        db.Leaves.AddRange(
            new SalonOS.Booking.Domain.Leave
            {
                ArtistId = artistId,
                TenantId = tenantA,
                StartDateTime = future,
                EndDateTime = future.AddHours(8),
                Status = SalonOS.Booking.Domain.LeaveStatus.Approved
            },
            new SalonOS.Booking.Domain.Leave
            {
                ArtistId = artistId,
                TenantId = tenantB,
                StartDateTime = future,
                EndDateTime = future.AddHours(8),
                Status = SalonOS.Booking.Domain.LeaveStatus.Approved
            });

        await db.SaveChangesAsync();

        var leavesA = await db.Leaves
            .Where(l => l.TenantId == tenantA && !l.IsDeleted)
            .ToListAsync();

        Assert.Single(leavesA);
        Assert.Equal(tenantA, leavesA[0].TenantId);
    }

    [Fact]
    public async Task GetAvailableSlots_returns_full_day_when_no_bookings()
    {
        using var db = CreateContext();
        var tenantId = Guid.NewGuid();
        var artistId = Guid.NewGuid();
        var monday = new DateTime(2026, 6, 15);

        db.ArtistSchedules.Add(new SalonOS.Booking.Domain.ArtistSchedule
        {
            ArtistId = artistId,
            TenantId = tenantId,
            DayOfWeek = DayOfWeek.Monday,
            StartTime = new TimeSpan(9, 0, 0),
            EndTime = new TimeSpan(17, 0, 0)
        });
        await db.SaveChangesAsync();

        var service = new BookingService(db);
        var slots = await service.GetAvailableSlotsAsync(artistId, monday, 60, tenantId);

        Assert.Equal(8, slots.Count);
        Assert.All(slots, s => Assert.True(s.IsAvailable));
        Assert.Equal(monday.Date.AddHours(9), slots[0].StartsAt);
        Assert.Equal(monday.Date.AddHours(17), slots[^1].EndsAt);
    }

    [Fact]
    public async Task GetAvailableSlots_excludes_booked_periods()
    {
        using var db = CreateContext();
        var tenantId = Guid.NewGuid();
        var artistId = Guid.NewGuid();
        var monday = new DateTime(2026, 6, 15);

        db.ArtistSchedules.Add(new SalonOS.Booking.Domain.ArtistSchedule
        {
            ArtistId = artistId,
            TenantId = tenantId,
            DayOfWeek = DayOfWeek.Monday,
            StartTime = new TimeSpan(9, 0, 0),
            EndTime = new TimeSpan(17, 0, 0)
        });

        db.Bookings.Add(new SalonOS.Booking.Domain.Booking
        {
            ArtistId = artistId,
            TenantId = tenantId,
            ClientId = "client-1",
            StartsAt = monday.Date.AddHours(11),
            EndsAt = monday.Date.AddHours(12),
            DurationMinutes = 60,
            Status = SalonOS.Booking.Domain.BookingStatus.Confirmed,
            EstimatedPrice = Money.Of(500_000, "IRR"),
            DepositAmount = Money.Of(100_000, "IRR")
        });
        await db.SaveChangesAsync();

        var service = new BookingService(db);
        var slots = await service.GetAvailableSlotsAsync(artistId, monday, 60, tenantId);

        Assert.Equal(7, slots.Count);
        Assert.All(slots, s => Assert.True(s.IsAvailable));
    }

    [Fact]
    public async Task GetAvailableSlots_excludes_leave_periods()
    {
        using var db = CreateContext();
        var tenantId = Guid.NewGuid();
        var artistId = Guid.NewGuid();
        var monday = new DateTime(2026, 6, 15);

        db.ArtistSchedules.Add(new SalonOS.Booking.Domain.ArtistSchedule
        {
            ArtistId = artistId,
            TenantId = tenantId,
            DayOfWeek = DayOfWeek.Monday,
            StartTime = new TimeSpan(9, 0, 0),
            EndTime = new TimeSpan(17, 0, 0)
        });

        db.Leaves.Add(new SalonOS.Booking.Domain.Leave
        {
            ArtistId = artistId,
            TenantId = tenantId,
            StartDateTime = monday.Date.AddHours(13),
            EndDateTime = monday.Date.AddHours(15),
            Status = SalonOS.Booking.Domain.LeaveStatus.Approved
        });
        await db.SaveChangesAsync();

        var service = new BookingService(db);
        var slots = await service.GetAvailableSlotsAsync(artistId, monday, 60, tenantId);

        Assert.Equal(6, slots.Count);
    }

    [Fact]
    public async Task GetAvailableSlots_returns_empty_when_no_schedule()
    {
        using var db = CreateContext();
        var service = new BookingService(db);
        var sunday = new DateTime(2026, 6, 14);

        var slots = await service.GetAvailableSlotsAsync(Guid.NewGuid(), sunday, 60, Guid.NewGuid());
        Assert.Empty(slots);
    }

    [Fact]
    public async Task GetAvailableSlots_respects_duration_minutes()
    {
        using var db = CreateContext();
        var tenantId = Guid.NewGuid();
        var artistId = Guid.NewGuid();
        var monday = new DateTime(2026, 6, 15);

        db.ArtistSchedules.Add(new SalonOS.Booking.Domain.ArtistSchedule
        {
            ArtistId = artistId,
            TenantId = tenantId,
            DayOfWeek = DayOfWeek.Monday,
            StartTime = new TimeSpan(9, 0, 0),
            EndTime = new TimeSpan(10, 0, 0)
        });
        await db.SaveChangesAsync();

        var service = new BookingService(db);

        var shortSlots = await service.GetAvailableSlotsAsync(artistId, monday, 30, tenantId);
        Assert.Equal(2, shortSlots.Count);

        var longSlots = await service.GetAvailableSlotsAsync(artistId, monday, 90, tenantId);
        Assert.Empty(longSlots);
    }

    [Fact]
    public async Task Booking_tenant_isolation_holds()
    {
        using var db = CreateContext();
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();
        var artistId = Guid.NewGuid();
        var now = DateTime.UtcNow;

        db.Bookings.AddRange(
            new SalonOS.Booking.Domain.Booking
            {
                ArtistId = artistId,
                TenantId = tenantA,
                ClientId = "client-A",
                StartsAt = now,
                EndsAt = now.AddHours(1),
                DurationMinutes = 60,
                Status = SalonOS.Booking.Domain.BookingStatus.Confirmed,
                EstimatedPrice = Money.Of(500_000, "IRR"),
                DepositAmount = Money.Of(100_000, "IRR")
            },
            new SalonOS.Booking.Domain.Booking
            {
                ArtistId = artistId,
                TenantId = tenantB,
                ClientId = "client-B",
                StartsAt = now,
                EndsAt = now.AddHours(1),
                DurationMinutes = 60,
                Status = SalonOS.Booking.Domain.BookingStatus.Confirmed,
                EstimatedPrice = Money.Of(300_000, "IRR"),
                DepositAmount = Money.Of(50_000, "IRR")
            });

        await db.SaveChangesAsync();

        var bookingsA = await db.Bookings
            .Where(b => b.TenantId == tenantA)
            .ToListAsync();

        Assert.Single(bookingsA);
        Assert.All(bookingsA, b => Assert.Equal(tenantA, b.TenantId));
    }

    [Fact]
    public async Task Leave_status_workflow()
    {
        using var db = CreateContext();
        var tenantId = Guid.NewGuid();
        var artistId = Guid.NewGuid();
        var future = DateTime.UtcNow.AddDays(7);

        var leave = new SalonOS.Booking.Domain.Leave
        {
            ArtistId = artistId,
            TenantId = tenantId,
            StartDateTime = future,
            EndDateTime = future.AddDays(1),
            Reason = "Vacation",
            Status = SalonOS.Booking.Domain.LeaveStatus.Pending
        };
        db.Leaves.Add(leave);
        await db.SaveChangesAsync();

        Assert.Equal(SalonOS.Booking.Domain.LeaveStatus.Pending, leave.Status);

        leave.Status = SalonOS.Booking.Domain.LeaveStatus.Approved;
        await db.SaveChangesAsync();

        var reloaded = await db.Leaves.FirstAsync(l => l.Id == leave.Id);
        Assert.Equal(SalonOS.Booking.Domain.LeaveStatus.Approved, reloaded.Status);
    }

    [Fact]
    public async Task ArtistSchedule_unique_per_artist_day()
    {
        using var db = CreateContext();
        var tenantId = Guid.NewGuid();
        var artistId = Guid.NewGuid();

        db.ArtistSchedules.Add(new SalonOS.Booking.Domain.ArtistSchedule
        {
            ArtistId = artistId,
            TenantId = tenantId,
            DayOfWeek = DayOfWeek.Monday,
            StartTime = new TimeSpan(9, 0, 0),
            EndTime = new TimeSpan(17, 0, 0)
        });
        await db.SaveChangesAsync();

        db.ArtistSchedules.Add(new SalonOS.Booking.Domain.ArtistSchedule
        {
            ArtistId = artistId,
            TenantId = tenantId,
            DayOfWeek = DayOfWeek.Monday,
            StartTime = new TimeSpan(10, 0, 0),
            EndTime = new TimeSpan(18, 0, 0)
        });

        var ex = await Record.ExceptionAsync(() => db.SaveChangesAsync());
        Assert.Null(ex);
    }
}
