using Microsoft.Extensions.Logging;
using Poshteh.Core.ApplicationService.Events;
using PoshtehStarter.Core.Domain.Ordering.Events;

namespace PoshtehStarter.Core.ApplicationService.Ordering.Events;

/// <summary>
/// Runs in-process right after <c>CreateOrderCommand</c>'s transaction commits:
/// commit -&gt; <c>IDomainEventDispatcher</c> -&gt; <c>IEventBus</c> -&gt; handler.
/// </summary>
public sealed partial class OrderCreatedEventHandler(ILogger<OrderCreatedEventHandler> logger) : IDomainEventHandler<OrderCreatedEvent>
{
    public Task HandleAsync(OrderCreatedEvent @event, CancellationToken cancellationToken = default)
    {
        LogOrderCreated(logger, @event.CustomerName, @event.ItemCount, @event.TotalAmount);

        return Task.CompletedTask;
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Order created for {CustomerName}: {ItemCount} item(s), total {TotalAmount:C}")]
    private static partial void LogOrderCreated(ILogger logger, string customerName, int itemCount, decimal totalAmount);
}
