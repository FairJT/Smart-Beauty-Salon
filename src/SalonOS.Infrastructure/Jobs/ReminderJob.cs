using Hangfire;
using SalonOS.Booking.Domain;

namespace SalonOS.Infrastructure.Jobs;

/// <summary>
/// Background job for sending appointment reminders.
/// Replaces the BackgroundService-based ReminderService.
/// </summary>
public class ReminderJob
{
    // TODO: Inject required services
    // private readonly IBookingService _bookingService;
    // private readonly INotificationService _notificationService;

    [AutomaticRetry(Attempts = 3)]
    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        // TODO: Implement reminder logic
        // 1. Find confirmed appointments starting within 2 hours
        // 2. Send SMS and in-app reminders
        // 3. Mark ReminderSent = true
        
        await Task.CompletedTask;
    }
}
