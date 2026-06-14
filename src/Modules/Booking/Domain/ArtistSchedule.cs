using SalonOS.Shared;

namespace SalonOS.Booking.Domain;

public class ArtistSchedule : TenantEntity
{
    public Guid ArtistId { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public bool IsActive { get; set; } = true;
}
