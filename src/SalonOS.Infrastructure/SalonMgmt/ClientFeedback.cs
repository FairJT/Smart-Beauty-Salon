using SalonOS.Shared;

namespace SalonOS.Infrastructure;

public enum ClientFeedbackType { Suggestion = 1, Complaint = 2 }
public enum ClientFeedbackStatus { Open = 1, InProgress = 2, Resolved = 3 }

public class ClientFeedback : TenantEntity
{
    public string ClientId { get; set; } = string.Empty;   // the user's id
    public ClientFeedbackType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Detail { get; set; }
    public ClientFeedbackStatus Status { get; set; } = ClientFeedbackStatus.Open;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}