using Poshteh.Core.Domain;

namespace Poshteh.Core.ApplicationService.Behaviors;

/// <summary>
/// Continues the pipeline to the next behavior, or to the handler if this is the last one.
/// </summary>
public delegate Task<Result<TResponse>> RequestHandlerDelegate<TResponse>();

/// <summary>
/// Attaches a cross-cutting concern (validation, authorization, transaction, logging, caching)
/// to command/query execution without modifying handlers. May short-circuit by not calling <paramref name="next"/>.
/// </summary>
public interface IPipelineBehavior<in TRequest, TResponse>
{
    Task<Result<TResponse>> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken = default);
}
