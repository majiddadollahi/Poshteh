using Microsoft.Extensions.DependencyInjection;
using Poshteh.Core.ApplicationService.Behaviors;
using Poshteh.Core.Domain;
using System.Reflection;

namespace Poshteh.Core.ApplicationService.Commands;

/// <summary>
/// Default <see cref="ICommandDispatcher"/>: resolves the matching <see cref="ICommandHandler{TCommand,TResponse}"/>
/// and any registered <see cref="IPipelineBehavior{TRequest,TResponse}"/> from the composition root, then runs the
/// command through the behavior chain (outermost registered first) down to the handler.
/// </summary>
public sealed class CommandDispatcher(IServiceProvider serviceProvider) : ICommandDispatcher
{
    private static readonly MethodInfo SendInternalMethod = typeof(CommandDispatcher)
        .GetMethod(nameof(SendInternal), BindingFlags.NonPublic | BindingFlags.Instance)!;

    public Task<Result<TResponse>> Send<TResponse>(ICommand<TResponse> command, CancellationToken cancellationToken = default)
    {
        var method = SendInternalMethod.MakeGenericMethod(command.GetType(), typeof(TResponse));
        return (Task<Result<TResponse>>)method.Invoke(this, [command, cancellationToken])!;
    }

    private async Task<Result<TResponse>> SendInternal<TCommand, TResponse>(TCommand command, CancellationToken cancellationToken)
        where TCommand : ICommand<TResponse>
    {
        var handler = serviceProvider.GetRequiredService<ICommandHandler<TCommand, TResponse>>();
        var behaviors = serviceProvider.GetServices<IPipelineBehavior<TCommand, TResponse>>().Reverse();

        RequestHandlerDelegate<TResponse> pipeline = () => handler.Handle(command, cancellationToken);

        foreach (var behavior in behaviors)
        {
            var next = pipeline;
            pipeline = () => behavior.Handle(command, next, cancellationToken);
        }

        return await pipeline();
    }
}
