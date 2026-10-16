using Microsoft.Extensions.DependencyInjection;
using Poshteh.Core.Domain;

namespace Poshteh.Core.ApplicationService.Events;

public interface IEventBus
{
    Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : IDomainEvent;

    Task PublishAsync(object @event, CancellationToken cancellationToken = default);
}

public interface IDomainEventHandler<in TEvent>
    where TEvent : IDomainEvent
{
    Task HandleAsync(TEvent @event, CancellationToken cancellationToken = default);
}

public sealed class InMemoryEventBus(IServiceProvider serviceProvider) : IEventBus
{
    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : IDomainEvent
    {
        if (@event is null)
        {
            return;
        }

        var handlers = serviceProvider.GetServices<IDomainEventHandler<TEvent>>();
        foreach (var handler in handlers)
        {
            await handler.HandleAsync(@event, cancellationToken);
        }
    }

    public async Task PublishAsync(object @event, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(@event);

        var eventType = @event.GetType();
        var genericMethod = typeof(InMemoryEventBus)
            .GetMethods()
            .Single(method => method.Name == nameof(PublishAsync)
                && method.IsGenericMethodDefinition
                && method.GetParameters().Length == 2);

        var method = genericMethod.MakeGenericMethod(eventType);
        await ((Task)method.Invoke(this, [@event, cancellationToken])!).ConfigureAwait(false);
    }
}
