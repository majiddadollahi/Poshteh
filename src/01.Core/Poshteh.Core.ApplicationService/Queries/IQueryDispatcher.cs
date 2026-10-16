using Poshteh.Core.Domain;

namespace Poshteh.Core.ApplicationService.Queries;

/// <summary>
/// Routes a query to its handler, through the registered pipeline behaviors,
/// without the caller knowing the handler type.
/// </summary>
public interface IQueryDispatcher
{
    Task<Result<TResponse>> Send<TResponse>(IQuery<TResponse> query, CancellationToken cancellationToken = default);
}
