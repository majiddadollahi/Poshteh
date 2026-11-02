using Poshteh.Core.Domain;

namespace Poshteh.Core.ApplicationService.Queries;

/// <summary>
/// Implements exactly one read use case for <typeparamref name="TQuery"/>.
/// </summary>
public interface IQueryHandler<in TQuery, TResponse>
    where TQuery : IQuery<TResponse>
{
    Task<Result<TResponse>> Handle(TQuery query, CancellationToken cancellationToken = default);
}
