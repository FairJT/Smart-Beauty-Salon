using SalonOS.Shared;

namespace SalonOS.Infrastructure;

/// <summary>
/// Outbox message entity for transactional outbox pattern.
/// Domain events are stored here in the same transaction as state changes.
/// A Hangfire job dispatches these messages to handlers.
/// </summary>
public class OutboxMessage
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string EventType { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ProcessedAt { get; set; }
    public string? Error { get; set; }
    public int RetryCount { get; set; } = 0;

    public static OutboxMessage From(DomainEvent domainEvent)
    {
        return new OutboxMessage
        {
            EventType = domainEvent.EventType,
            Payload = System.Text.Json.JsonSerializer.Serialize(domainEvent),
            CreatedAt = domainEvent.OccurredOn
        };
    }
}
