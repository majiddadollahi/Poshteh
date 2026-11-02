using Poshteh.Core.Domain;

namespace Poshteh.Core.ApplicationService.Commands;

/// <summary>
/// Implements exactly one state-changing use case for <typeparamref name="TCommand"/>.
/// </summary>
public interface ICommandHandler<in TCommand, TResponse>
    where TCommand : ICommand<TResponse>
{
    Task<Result<TResponse>> Handle(TCommand command, CancellationToken cancellationToken = default);
}

/// <summary>
/// Implements exactly one state-changing use case that produces no meaningful return value.
/// </summary>
public interface ICommandHandler<in TCommand> : ICommandHandler<TCommand, Unit>
    where TCommand : ICommand
{
}
