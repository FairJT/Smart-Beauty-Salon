namespace SalonOS.Shared;

/// <summary>
/// Interface for entities that raise domain events.
/// Entities implementing this can dequeue events that will be persisted to the outbox.
/// </summary>
public interface IHasDomainEvents
{
    IReadOnlyList<DomainEvent> DomainEvents { get; }
    void RaiseDomainEvent(DomainEvent domainEvent);
    IReadOnlyList<DomainEvent> DequeueEvents();
}
