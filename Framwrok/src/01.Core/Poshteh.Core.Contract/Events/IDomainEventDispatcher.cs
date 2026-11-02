using Poshteh.Core.Domain;

namespace Poshteh.Core.Contract;

/// <summary>
/// Bridges the write-side persistence adapter (which must not depend on Core.ApplicationService) to the
/// actual in-process event bus. Invoked by <see cref="IUnitOfWork"/> after a successful commit, so that
/// domain events raised during the transaction are handed to their <c>IDomainEventHandler&lt;T&gt;</c>s.
/// </summary>
public interface IDomainEventDispatcher
{
    Task DispatchAsync(IReadOnlyList<IDomainEvent> domainEvents, CancellationToken cancellationToken = default);
}
