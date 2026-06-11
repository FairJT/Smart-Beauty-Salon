namespace SmartSalon.Models
{
    public class Notification
    {
        public int Id { get; set; }
        public string UserId { get; set; } = "";
        public string Title { get; set; } = "";
        public string Message { get; set; } = "";
        public string Type { get; set; } = "info"; // info, success, warning, error
        public bool IsRead { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation
        public ApplicationUser? User { get; set; }
    }
}