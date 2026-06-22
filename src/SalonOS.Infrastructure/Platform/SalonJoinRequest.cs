namespace SalonOS.Infrastructure;

public enum JoinRequestStatus { Pending = 1, Approved = 2, Rejected = 3 }

public class SalonJoinRequest
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string SalonName { get; set; } = string.Empty;
    public string OwnerName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? City { get; set; }
    public string? Note { get; set; }
    public JoinRequestStatus Status { get; set; } = JoinRequestStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}