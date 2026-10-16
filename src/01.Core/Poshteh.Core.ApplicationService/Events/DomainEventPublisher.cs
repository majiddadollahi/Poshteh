using Poshteh.Core.Domain;

namespace Poshteh.Core.ApplicationService.Events;

public interface IDomainEventPublisher
{
    Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : IDomainEvent;
}

public sealed class DomainEventPublisher(IEventBus eventBus) : IDomainEventPublisher
{
    public Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : IDomainEvent
    {
        return eventBus.PublishAsync(@event, cancellationToken);
    }
}
