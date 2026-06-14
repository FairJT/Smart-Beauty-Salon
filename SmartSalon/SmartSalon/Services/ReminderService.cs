using Microsoft.EntityFrameworkCore;
using SmartSalon.Data;
using SmartSalon.Models;
using SmartSalon.Services;

namespace SmartSalon.Services
{
    public class ReminderService : BackgroundService
    {
        private readonly IServiceProvider _services;
        private readonly ILogger<ReminderService> _logger;

        public ReminderService(IServiceProvider services, ILogger<ReminderService> logger)
        {
            _services = services;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Reminder Service started");

            while (!stoppingToken.IsCancellationRequested)
            {
                await SendReminders();
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }

        private async Task SendReminders()
        {
            using var scope = _services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var sms = scope.ServiceProvider.GetRequiredService<ISmsService>();
            var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

            var reminderTime = DateTime.UtcNow.AddHours(2);
            var from = reminderTime.AddMinutes(-30);
            var to = reminderTime.AddMinutes(30);

            var appointments = await db.Appointments
                .Include(a => a.Client)
                .Include(a => a.Salon)
                .Include(a => a.Artist).ThenInclude(ar => ar!.User)
                .Where(a => a.Status == AppointmentStatus.Confirmed
                         && !a.ReminderSent
                         && a.StartTime >= from
                         && a.StartTime <= to)
                .ToListAsync();

            foreach (var apt in appointments)
            {
                if (apt.Client == null) continue;

                var artistName = (apt.Artist?.User?.FirstName ?? "") + " " +
                                 (apt.Artist?.User?.LastName ?? "");
                var dateTime = apt.StartTime.ToString("yyyy/MM/dd HH:mm");
                var salonName = apt.Salon?.Name ?? "";

                // Send SMS if phone number available
                if (apt.Client.PhoneNumber != null)
                {
                    var smsSent = await sms.SendAppointmentReminderAsync(
                        apt.Client.PhoneNumber,
                        apt.Client.FirstName,
                        salonName,
                        artistName,
                        dateTime
                    );

                    if (smsSent)
                    {
                        _logger.LogInformation(
                            "SMS reminder sent to {mobile}", apt.Client.PhoneNumber);
                    }
                }

                // Send in-app notification
                await notificationService.SendAppointmentReminderAsync(
                    apt.Client.Id,
                    salonName,
                    dateTime
                );

                apt.ReminderSent = true;
                _logger.LogInformation(
                    "In-app reminder sent to user {userId}", apt.Client.Id);
            }

            await db.SaveChangesAsync();
        }
    }
}
