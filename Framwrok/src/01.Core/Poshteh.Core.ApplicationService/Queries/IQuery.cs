namespace Poshteh.Core.ApplicationService.Queries;

/// <summary>
/// Represents a request to read data, shaped for its consumer rather than for the domain model.
/// Must never be used to trigger a side effect.
/// </summary>
public interface IQuery<TResponse>
{
}
