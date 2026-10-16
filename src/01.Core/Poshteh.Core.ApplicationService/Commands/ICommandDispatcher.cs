using Poshteh.Core.Domain;

namespace Poshteh.Core.ApplicationService.Commands;

/// <summary>
/// Routes a command to its handler, through the registered pipeline behaviors,
/// without the caller knowing the handler type.
/// </summary>
public interface ICommandDispatcher
{
    Task<Result<TResponse>> Send<TResponse>(ICommand<TResponse> command, CancellationToken cancellationToken = default);
}
