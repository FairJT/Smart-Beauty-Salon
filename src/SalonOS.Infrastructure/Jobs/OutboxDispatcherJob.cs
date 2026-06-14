using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SalonOS.Booking.Domain;
using SalonOS.Shared;

namespace SalonOS.Infrastructure.Jobs;

public class OutboxDispatcherJob
{
    private static readonly Dictionary<string, Type> HandlerMap = new()
    {
        [nameof(BookingCompleted)] = typeof(EventHandlers.BookingCompletedHandler),
        [nameof(BookingCancelled)] = typeof(EventHandlers.BookingCancelledHandler),
    };

    private static readonly Dictionary<string, Type> EventTypeMap = new()
    {
        [nameof(BookingCompleted)] = typeof(BookingCompleted),
        [nameof(BookingCancelled)] = typeof(BookingCancelled),
    };

    private readonly AppDbContext _context;
    private readonly IServiceProvider _serviceProvider;

    public OutboxDispatcherJob(AppDbContext context, IServiceProvider serviceProvider)
    {
        _context = context;
        _serviceProvider = serviceProvider;
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var messages = await _context.OutboxMessages
            .Where(m => m.ProcessedAt == null)
            .OrderBy(m => m.CreatedAt)
            .Take(50)
            .ToListAsync(cancellationToken);

        foreach (var message in messages)
        {
            try
            {
                await DispatchAsync(message);
                message.ProcessedAt = DateTime.UtcNow;
                message.Error = null;
            }
            catch (Exception ex)
            {
                message.Error = ex.Message;
                message.RetryCount++;
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task DispatchAsync(OutboxMessage message)
    {
        if (!HandlerMap.TryGetValue(message.EventType, out var handlerType))
            return;

        if (!EventTypeMap.TryGetValue(message.EventType, out var eventType))
            return;

        var handler = _serviceProvider.GetRequiredService(handlerType);
        var handleMethod = handlerType.GetMethod("HandleAsync");
        if (handleMethod == null)
            return;

        var domainEvent = JsonSerializer.Deserialize(message.Payload, eventType);
        if (domainEvent != null)
        {
            var task = (Task?)handleMethod.Invoke(handler, [domainEvent]);
            if (task != null)
                await task;
        }
    }
}
