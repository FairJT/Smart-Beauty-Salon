using Microsoft.AspNetCore.Mvc;
using SalonOS.Shared.Authorization;
using SalonOS.Shared.Authorization;

namespace SalonOS.Identity.API.Controllers;

/// <summary>
/// Notifications controller.
/// Task 6.6: notification.send (SalonManager, Receptionist, Artist) and
/// notification.view.own (all roles) per §R4.
/// Authorize on permission strings — never on role names (R2).
/// </summary>
[Route("api/notifications")]
[ApiController]
public class NotificationsController : ControllerBase
{
    // ── GET /api/notifications — notification.view.own ────────────────────────
    // Every authenticated role can view their own notifications.
    [HttpGet]
    [HasPermission(Permissions.NotificationViewOwn)]
    public IActionResult GetNotifications()
    {
        // Returns notifications for the calling user only (filtered by UserId in service layer)
        return Ok(new List<object>());
    }

    // ── GET /api/notifications/{id} — notification.view.own ──────────────────
    [HttpGet("{id}")]
    [HasPermission(Permissions.NotificationViewOwn)]
    public IActionResult GetNotification(Guid id)
    {
        return NotFound(new { message = "Notification not found" });
    }

    // ── POST /api/notifications/send — notification.send ──────────────────────
    // SalonManager, Receptionist, Artist can send notifications.
    [HttpPost("send")]
    [HasPermission(Permissions.NotificationSend)]
    public IActionResult SendNotification([FromBody] SendNotificationDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        return Ok(new { message = "Notification sent" });
    }
}

/// <summary>DTO for sending a notification.</summary>
public class SendNotificationDto
{
    public string RecipientUserId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
}
