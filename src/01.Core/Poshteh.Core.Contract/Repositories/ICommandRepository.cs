using Poshteh.Core.Domain;

namespace Poshteh.Core.Contract;

/// <summary>
/// Loads and persists aggregate roots without naming a storage technology.
/// Deliberately minimal: get-by-id and add are the only operations every write use case needs;
/// tracked entities are updated implicitly through the Unit of Work commit, not through this port.
/// </summary>
public interface ICommandRepository<TEntity> where TEntity : AggregateRoot
{
    Task<TEntity?> GetByIdAsync(long id, CancellationToken cancellationToken = default);

    Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);
}