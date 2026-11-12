namespace Poshteh.Core.Contract.Localization;

/// <summary>
/// The normalized culture (e.g. <c>fa</c>, <c>en</c>) of the current request. Outside an HTTP request
/// (background jobs, workers) implementations return the configured default culture.
/// </summary>
public interface IRequestCultureAccessor
{
    string Culture { get; }
}
