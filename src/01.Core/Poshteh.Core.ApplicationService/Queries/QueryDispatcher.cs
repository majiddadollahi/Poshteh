using Microsoft.Extensions.DependencyInjection;
using Poshteh.Core.ApplicationService.Behaviors;
using Poshteh.Core.Domain;
using System.Reflection;

namespace Poshteh.Core.ApplicationService.Queries;

/// <summary>
/// Default <see cref="IQueryDispatcher"/>: resolves the matching <see cref="IQueryHandler{TQuery,TResponse}"/>
/// and any registered <see cref="IPipelineBehavior{TRequest,TResponse}"/> from the composition root, then runs the
/// query through the behavior chain (outermost registered first) down to the handler.
/// </summary>
public sealed class QueryDispatcher(IServiceProvider serviceProvider) : IQueryDispatcher
{
    private static readonly MethodInfo SendInternalMethod = typeof(QueryDispatcher)
        .GetMethod(nameof(SendInternal), BindingFlags.NonPublic | BindingFlags.Instance)!;

    public Task<Result<TResponse>> Send<TResponse>(IQuery<TResponse> query, CancellationToken cancellationToken = default)
    {
        var method = SendInternalMethod.MakeGenericMethod(query.GetType(), typeof(TResponse));
        return (Task<Result<TResponse>>)method.Invoke(this, [query, cancellationToken])!;
    }

    private async Task<Result<TResponse>> SendInternal<TQuery, TResponse>(TQuery query, CancellationToken cancellationToken)
        where TQuery : IQuery<TResponse>
    {
        var handler = serviceProvider.GetRequiredService<IQueryHandler<TQuery, TResponse>>();
        var behaviors = serviceProvider.GetServices<IPipelineBehavior<TQuery, TResponse>>().Reverse();

        RequestHandlerDelegate<TResponse> pipeline = () => handler.Handle(query, cancellationToken);

        foreach (var behavior in behaviors)
        {
            var next = pipeline;
            pipeline = () => behavior.Handle(query, next, cancellationToken);
        }

        return await pipeline();
    }
}
