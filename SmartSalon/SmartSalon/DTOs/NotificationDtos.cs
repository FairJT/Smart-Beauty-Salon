namespace SmartSalon.DTOs
{
    public class NotificationListItemDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Type { get; set; } = "info";
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class NotificationsResponseDto
    {
        public List<NotificationListItemDto> Notifications { get; set; } = new();
        public int UnreadCount { get; set; }
    }
}
