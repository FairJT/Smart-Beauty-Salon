using SalonOS.Shared;

namespace SalonOS.Booking.Domain;

/// <summary>
/// Booking completed domain event.
/// Raised when a booking is completed.
/// </summary>
public class BookingCompleted : DomainEvent
{
    public Guid BookingId { get; }
    public Guid TenantId { get; }
    public Guid ArtistId { get; }
    public string ClientId { get; }
    public Money FinalPrice { get; }

    public BookingCompleted(Guid bookingId, Guid tenantId, Guid artistId, string clientId, Money finalPrice)
    {
        BookingId = bookingId;
        TenantId = tenantId;
        ArtistId = artistId;
        ClientId = clientId;
        FinalPrice = finalPrice;
    }

    public override string EventType => nameof(BookingCompleted);
}
