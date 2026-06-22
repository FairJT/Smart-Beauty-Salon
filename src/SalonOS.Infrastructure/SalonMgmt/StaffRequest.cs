using SalonOS.Shared;

namespace SalonOS.Infrastructure;

public enum StaffRequestType { Issue = 1, Equipment = 2 }
public enum StaffRequestStatus { Open = 1, InProgress = 2, Resolved = 3 }

public class StaffRequest : TenantEntity
{
    public Guid ArtistId { get; set; }
    public StaffRequestType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Detail { get; set; }
    public StaffRequestStatus Status { get; set; } = StaffRequestStatus.Open;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}