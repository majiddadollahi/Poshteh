using Poshteh.Core.Contract;
using Poshteh.Core.Domain;

namespace Poshteh.Core.ApplicationService.Events;

/// <summary>
/// Default <see cref="IDomainEventDispatcher"/>: forwards each event to <see cref="IEventBus"/>, which resolves
/// and invokes the matching <see cref="IDomainEventHandler{TEvent}"/>s for its runtime type.
/// </summary>
public sealed class EventBusDomainEventDispatcher(IEventBus eventBus) : IDomainEventDispatcher
{
    public async Task DispatchAsync(IReadOnlyList<IDomainEvent> domainEvents, CancellationToken cancellationToken = default)
    {
        foreach (var domainEvent in domainEvents)
        {
            // Must go through the (object, ct) overload: domainEvent's static type here is IDomainEvent, and
            // the generic PublishAsync<TEvent> overload would otherwise infer TEvent = IDomainEvent instead of
            // the event's real runtime type, silently missing every IDomainEventHandler<TConcreteEvent>.
            await eventBus.PublishAsync((object)domainEvent, cancellationToken);
        }
    }
}
