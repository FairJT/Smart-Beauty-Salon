using Microsoft.EntityFrameworkCore.Diagnostics;
using SalonOS.Shared;

namespace SalonOS.Infrastructure;

/// <summary>
/// SaveChanges interceptor that captures domain events and stores them in the outbox.
/// This ensures domain events are persisted in the same transaction as state changes.
/// </summary>
public class OutboxInterceptor : SaveChangesInterceptor
{
    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        var context = eventData.Context;
        if (context == null)
            return await base.SavingChangesAsync(eventData, result, cancellationToken);

        var entries = context.ChangeTracker.Entries<IHasDomainEvents>()
            .SelectMany(e => e.Entity.DequeueEvents())
            .ToList();

        foreach (var domainEvent in entries)
        {
            var outboxMessage = OutboxMessage.From(domainEvent);
            context.Set<OutboxMessage>().Add(outboxMessage);
        }

        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}
