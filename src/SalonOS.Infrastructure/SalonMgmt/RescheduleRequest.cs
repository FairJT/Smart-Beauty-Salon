using SalonOS.Shared;

namespace SalonOS.Infrastructure;

public enum RescheduleStatus { Pending = 1, Approved = 2, Rejected = 3 }

public class RescheduleRequest : TenantEntity
{
    public Guid BookingId { get; set; }
    public Guid ArtistId { get; set; }
    public DateTime ProposedStart { get; set; }
    public string? Reason { get; set; }
    public RescheduleStatus Status { get; set; } = RescheduleStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}