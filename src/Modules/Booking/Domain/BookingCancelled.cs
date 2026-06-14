using SalonOS.Shared;

namespace SalonOS.Booking.Domain;

/// <summary>
/// Booking cancelled domain event.
/// Raised when a booking is cancelled.
/// </summary>
public class BookingCancelled : DomainEvent
{
    public Guid BookingId { get; }
    public Guid TenantId { get; }
    public string ClientId { get; }
    public string? Reason { get; }

    public BookingCancelled(Guid bookingId, Guid tenantId, string clientId, string? reason = null)
    {
        BookingId = bookingId;
        TenantId = tenantId;
        ClientId = clientId;
        Reason = reason;
    }

    public override string EventType => nameof(BookingCancelled);
}
