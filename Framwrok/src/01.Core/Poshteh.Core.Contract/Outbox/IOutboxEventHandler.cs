namespace Poshteh.Core.Contract.Outbox;

public interface IOutboxEventHandler
{
    string EventTypeName { get; }

    Task HandleAsync(string eventPayload, CancellationToken cancellationToken = default);
}
