using Microsoft.Extensions.Logging;
using Poshteh.Core.ApplicationService.Events;
using Poshteh.Sample.Ordering.Domain.Events;

namespace Poshteh.Sample.Ordering.Application.Events;

/// <summary>
/// Proves the commit -&gt; <c>IDomainEventDispatcher</c> -&gt; <c>IEventBus</c> -&gt; handler wiring actually fires:
/// this runs in-process, right after <c>CreateOrderCommand</c>'s transaction commits.
/// </summary>
public sealed class OrderCreatedEventHandler(ILogger<OrderCreatedEventHandler> logger) : IDomainEventHandler<OrderCreatedEvent>
{
    public Task HandleAsync(OrderCreatedEvent @event, CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "Order created for {CustomerName}: {ItemCount} item(s), total {TotalAmount:C}",
            @event.CustomerName,
            @event.ItemCount,
            @event.TotalAmount);

        return Task.CompletedTask;
    }
}
