using Microsoft.EntityFrameworkCore;
using SmartSalon.Data;
using SmartSalon.DTOs;
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
        Task<List<NotificationListItemDto>> GetMineAsync(string userId, int take = 50);
        Task<int> GetUnreadCountAsync(string userId);
        Task<bool> MarkAsReadAsync(int id, string userId);
        Task<int> MarkAllAsReadAsync(string userId);
        Task<bool> DeleteAsync(int id, string userId);
    }

    public class NotificationService : INotificationService
    {
        private readonly ApplicationDbContext _db;

        public NotificationService(ApplicationDbContext db) => _db = db;

        public async Task SendAsync(string userId, string title, string message, string type = "info")
        {
            var notification = new Notification
            {
                UserId = userId,
                Title = title,
                Message = message,
                Type = type,
                CreatedAt = DateTime.UtcNow
            };

            _db.Notifications.Add(notification);
            await _db.SaveChangesAsync();
        }

        public async Task SendAppointmentConfirmedAsync(
            string userId, string salonName, string dateTime)
        {
            await SendAsync(
                userId,
                "Appointment Confirmed",
                $"Your appointment at {salonName} for {dateTime} has been confirmed.",
                "success"
            );
        }

        public async Task SendAppointmentCancelledAsync(
            string userId, string salonName)
        {
            await SendAsync(
                userId,
                "Appointment Cancelled",
                $"Your appointment at {salonName} has been cancelled.",
                "error"
            );
        }

        public async Task SendAppointmentReminderAsync(
            string userId, string salonName, string dateTime)
        {
            await SendAsync(
                userId,
                "Appointment Reminder",
                $"Your appointment at {salonName} for {dateTime} is coming up.",
                "warning"
            );
        }

        public async Task SendNewAppointmentToManagerAsync(
            string userId, string clientName, string dateTime)
        {
            await SendAsync(
                userId,
                "New Appointment",
                $"Client {clientName} booked an appointment for {dateTime}.",
                "info"
            );
        }

        public async Task<List<NotificationListItemDto>> GetMineAsync(string userId, int take = 50)
        {
            return await _db.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Take(take)
                .Select(n => new NotificationListItemDto
                {
                    Id = n.Id,
                    Title = n.Title,
                    Message = n.Message,
                    Type = n.Type,
                    IsRead = n.IsRead,
                    CreatedAt = n.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<int> GetUnreadCountAsync(string userId)
        {
            return await _db.Notifications
                .CountAsync(n => n.UserId == userId && !n.IsRead);
        }

        public async Task<bool> MarkAsReadAsync(int id, string userId)
        {
            var notification = await _db.Notifications
                .FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);

            if (notification == null) return false;

            notification.IsRead = true;
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<int> MarkAllAsReadAsync(string userId)
        {
            var unread = await _db.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();

            unread.ForEach(n => n.IsRead = true);
            await _db.SaveChangesAsync();
            return unread.Count;
        }

        public async Task<bool> DeleteAsync(int id, string userId)
        {
            var notification = await _db.Notifications
                .FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);

            if (notification == null) return false;

            _db.Notifications.Remove(notification);
            await _db.SaveChangesAsync();
            return true;
        }
    }
}
