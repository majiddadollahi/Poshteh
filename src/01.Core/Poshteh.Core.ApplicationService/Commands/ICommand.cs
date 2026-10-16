namespace Poshteh.Core.ApplicationService.Commands;

/// <summary>
/// Represents an intent to change system state and produce <typeparamref name="TResponse"/>.
/// </summary>
public interface ICommand<TResponse>
{
}

/// <summary>
/// A command that changes state without producing a meaningful return value.
/// </summary>
public interface ICommand : ICommand<Unit>
{
}
