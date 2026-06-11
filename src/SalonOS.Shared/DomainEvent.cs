namespace SalonOS.Shared;

/// <summary>
/// Base class for all domain events.
/// Domain events are raised by entities and dispatched via the outbox pattern.
/// </summary>
public abstract class DomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public abstract string EventType { get; }
}
