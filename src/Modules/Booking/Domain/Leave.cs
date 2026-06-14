using SalonOS.Shared;

namespace SalonOS.Booking.Domain;

public enum LeaveStatus
{
    Pending = 1,
    Approved = 2,
    Rejected = 3
}

public class Leave : TenantEntity
{
    public Guid ArtistId { get; set; }
    public DateTime StartDateTime { get; set; }
    public DateTime EndDateTime { get; set; }
    public string? Reason { get; set; }
    public LeaveStatus Status { get; set; } = LeaveStatus.Pending;
}
