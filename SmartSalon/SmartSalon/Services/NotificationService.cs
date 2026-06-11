using Microsoft.EntityFrameworkCore;
using SmartSalon.Data;
using SmartSalon.Models;

namespace SmartSalon.Services
{
    public interface INotificationService
    {
        Task SendAsync(string userId, string title, string message, string type = "info");
        Task SendAppointmentConfirmedAsync(string userId, string salonName, string dateTime);
        Task SendAppointmentCancelledAsync(string userId, string salonName);
        Task SendAppointmentReminderAsync(string userId, string salonName, string dateTime);
        Task SendNewAppointmentToManagerAsync(string userId, string clientName, string dateTime);
    }

    public class NotificationService : INotificationService
    {
        private readonly ApplicationDbContext _db;

        public NotificationService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task SendAsync(string userId, string title, string message, string type = "info")
        {
            var notification = new Notification
            {
                UserId = userId,
                Title = title,
                Message = message,
                Type = type,
                CreatedAt = DateTime.Now
            };

            _db.Notifications.Add(notification);
            await _db.SaveChangesAsync();
        }

        public async Task SendAppointmentConfirmedAsync(
            string userId, string salonName, string dateTime)
        {
            await SendAsync(
                userId,
                "نوبت تایید شد ✅",
                $"نوبت شما در {salonName} برای {dateTime} تایید شد.",
                "success"
            );
        }

        public async Task SendAppointmentCancelledAsync(
            string userId, string salonName)
        {
            await SendAsync(
                userId,
                "نوبت لغو شد ❌",
                $"نوبت شما در {salonName} لغو شد.",
                "error"
            );
        }

        public async Task SendAppointmentReminderAsync(
            string userId, string salonName, string dateTime)
        {
            await SendAsync(
                userId,
                "یادآوری نوبت 🔔",
                $"نوبت شما در {salonName} برای {dateTime} نزدیک است.",
                "warning"
            );
        }

        public async Task SendNewAppointmentToManagerAsync(
            string userId, string clientName, string dateTime)
        {
            await SendAsync(
                userId,
                "نوبت جدید 📅",
                $"مشتری {clientName} برای {dateTime} نوبت گرفت.",
                "info"
            );
        }
    }
}