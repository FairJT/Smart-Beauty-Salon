using Microsoft.EntityFrameworkCore;
using SmartSalon.Data;
using SmartSalon.Models;

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
                // هر ۱ ساعت یکبار چک کن
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }

        private async Task SendReminders()
        {
            using var scope = _services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var sms = scope.ServiceProvider.GetRequiredService<ISmsService>();

            // نوبت‌هایی که ۲ ساعت دیگر شروع می‌شوند
            var reminderTime = DateTime.Now.AddHours(2);
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
                if (apt.Client?.PhoneNumber == null) continue;

                var artistName = apt.Artist?.User?.FirstName + " " +
                                 apt.Artist?.User?.LastName;
                var dateTime = apt.StartTime.ToString("yyyy/MM/dd HH:mm");

                var sent = await sms.SendAppointmentReminderAsync(
                    apt.Client.PhoneNumber,
                    apt.Client.FirstName,
                    apt.Salon?.Name ?? "",
                    artistName,
                    dateTime
                );

                if (sent)
                {
                    apt.ReminderSent = true;
                    _logger.LogInformation(
                        "Reminder sent to {mobile}", apt.Client.PhoneNumber);
                }
            }

            await db.SaveChangesAsync();
        }
    }
}