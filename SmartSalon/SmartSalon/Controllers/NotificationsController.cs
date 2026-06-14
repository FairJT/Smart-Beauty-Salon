using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartSalon.DTOs;
using SmartSalon.Services;
using System.Security.Claims;

namespace SmartSalon.Controllers
{
    [Route("api/notifications")]
    [ApiController]
    [Authorize]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationsController(INotificationService notificationService)
            => _notificationService = notificationService;

        [HttpGet]
        public async Task<IActionResult> GetMine()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var notifications = await _notificationService.GetMineAsync(userId);
            var unreadCount = await _notificationService.GetUnreadCountAsync(userId);

            return Ok(new NotificationsResponseDto
            {
                Notifications = notifications,
                UnreadCount = unreadCount
            });
        }

        [HttpPut("{id:int}/read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var success = await _notificationService.MarkAsReadAsync(id, userId);

            if (!success) return NotFound();
            return Ok(new { message = "Marked as read" });
        }

        [HttpPut("read-all")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var count = await _notificationService.MarkAllAsReadAsync(userId);

            return Ok(new { message = "All marked as read", count });
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var success = await _notificationService.DeleteAsync(id, userId);

            if (!success) return NotFound();
            return Ok(new { message = "Deleted" });
        }

        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var count = await _notificationService.GetUnreadCountAsync(userId);

            return Ok(new { count });
        }
    }
}
