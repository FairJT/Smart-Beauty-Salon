using SalonOS.Shared;

namespace SalonOS.Booking.Domain;

/// <summary>
/// Booking entity - TENANT entity.
/// Represents a customer booking at a salon.
/// </summary>
public class Booking : TenantEntity, IHasDomainEvents
{
    public string ClientId { get; set; } = string.Empty;
    public Guid ArtistId { get; set; }
    public Guid ServiceId { get; set; }
    public DateTime StartsAt { get; set; }
    public DateTime EndsAt { get; set; }
    public int DurationMinutes { get; set; }
    public Money EstimatedPrice { get; set; }
    public Money? FinalPrice { get; set; }
    public Money DepositAmount { get; set; }
    public BookingStatus Status { get; set; } = BookingStatus.Pending;
    public DateTime? CheckedInAt { get; set; }   // set when the client arrives (item 6/7)
    public string? Notes { get; set; }
    public bool IsRated { get; set; } = false;
    public int? Rating { get; set; }
    public string? Comment { get; set; }
    public bool ReminderSent { get; set; } = false;
    public string? CustomerSelectionSnapshot { get; set; } // jsonb for option snapshot
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Foreign keys only - navigation properties handled by EF Core or removed for module independence
    // public string ClientId { get; set; } // Already defined above
    // public Guid ArtistId { get; set; } // Already defined above
    // public Guid ServiceId { get; set; } // Already defined above
    
    // Domain events
    private readonly List<DomainEvent> _domainEvents = new();
    public IReadOnlyList<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();
    
    public void RaiseDomainEvent(DomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }
    
    public IReadOnlyList<DomainEvent> DequeueEvents()
    {
        var events = _domainEvents.ToList();
        _domainEvents.Clear();
        return events;
    }
}

/// <summary>
/// Booking status enum.
/// </summary>
public enum BookingStatus
{
    Pending = 1,
    Confirmed = 2,
    InProgress = 3,
    Completed = 4,
    Cancelled = 5,
    NoShow = 6
}
