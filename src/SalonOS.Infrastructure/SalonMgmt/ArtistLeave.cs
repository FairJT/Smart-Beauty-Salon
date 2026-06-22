using SalonOS.Shared;

namespace SalonOS.Infrastructure;

public enum ArtistLeaveStatus { Pending = 1, Approved = 2, Rejected = 3 }

public class ArtistLeave : TenantEntity
{
    public Guid ArtistId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Reason { get; set; } = string.Empty;
    public ArtistLeaveStatus Status { get; set; } = ArtistLeaveStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}