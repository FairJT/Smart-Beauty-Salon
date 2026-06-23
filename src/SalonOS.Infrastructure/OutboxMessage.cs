using System;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SalonOS.Shared;

namespace SalonOS.Infrastructure
{
    /// <summary>
    /// Outbox message entity used by Hangfire background jobs.
    /// </summary>
    public class OutboxMessage
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? Payload { get; set; }
        public bool Dispatched { get; set; }
        public DateTime? ProcessedAt { get; set; }
        public string? EventType { get; set; }
        public string? Error { get; set; }
        public int RetryCount { get; set; }

        /// <summary>
        /// Creates an OutboxMessage from a domain event.
        /// </summary>
        public static OutboxMessage From(DomainEvent domainEvent)
        {
            return new OutboxMessage
            {
                Id = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow,
                Payload = JsonSerializer.Serialize(domainEvent, domainEvent.GetType()),
                EventType = domainEvent.GetType().AssemblyQualifiedName,
                Dispatched = false,
                Error = null,
                RetryCount = 0
            };
        }
    }

    public static class OutboxModelBuilder
    {
        public static void Configure(ModelBuilder builder)
        {
            builder.Entity<OutboxMessage>(e =>
            {
                e.ToTable("OutboxMessages");
                e.HasKey(o => o.Id);
                e.Property(o => o.CreatedAt).IsRequired();
                e.Property(o => o.Payload).HasColumnType("nvarchar(max)");
                e.Property(o => o.Dispatched).HasDefaultValue(false);
                e.Property(o => o.ProcessedAt);
                e.Property(o => o.EventType).HasColumnType("nvarchar(200)");
                e.Property(o => o.Error).HasColumnType("nvarchar(max)");
                e.Property(o => o.RetryCount).HasDefaultValue(0);
            });
        }
    }
}