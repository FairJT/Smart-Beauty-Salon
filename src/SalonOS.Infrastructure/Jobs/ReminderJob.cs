using Microsoft.EntityFrameworkCore;
using SalonOS.Booking.Domain;
using SalonOS.Booking.Infrastructure;

namespace SalonOS.Infrastructure.Jobs;

public class ReminderJob
{
    private readonly BookingDbContext _context;

    public ReminderJob(BookingDbContext context)
    {
        _context = context;
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var upcoming = now.AddHours(2);

        var bookings = await _context.Bookings
            .Where(b => b.Status == BookingStatus.Confirmed
                     && !b.ReminderSent
                     && b.StartsAt <= upcoming
                     && b.StartsAt > now)
            .ToListAsync(cancellationToken);

        foreach (var booking in bookings)
        {
            booking.ReminderSent = true;
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
