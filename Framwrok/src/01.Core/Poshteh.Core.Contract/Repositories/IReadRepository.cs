namespace Poshteh.Core.Contract;

/// <summary>
/// Fetches denormalized read models optimized for a specific query, independent of the write model.
/// Bypasses the aggregate entirely — the read side never goes through <see cref="ICommandRepository{TEntity}"/>.
/// </summary>
public interface IReadRepository<TReadModel> where TReadModel : class
{
    Task<TReadModel?> GetByIdAsync(long id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TReadModel>> ListAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// A no-tracking queryable for use-case-specific shaping (filtering, projection, paging).
    /// Query handlers own their own query shape; this port only owns the storage seam.
    /// </summary>
    IQueryable<TReadModel> Query();
}
