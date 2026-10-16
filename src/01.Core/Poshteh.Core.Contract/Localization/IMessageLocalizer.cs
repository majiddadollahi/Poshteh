using System.Globalization;
using Poshteh.Core.Domain;

namespace Poshteh.Core.Contract.Localization;

/// <summary>
/// Resolves stable message keys at an application boundary without coupling Core to a web framework.
/// </summary>
public interface IMessageLocalizer
{
    string Localize(
        string key,
        IReadOnlyList<object>? args = null,
        string? explicitMessage = null,
        CultureInfo? culture = null);

    string Localize(Error error, CultureInfo? culture = null);

    /// <summary>
    /// Resolves a key without falling back to the unexpected-error message when no resource exists.
    /// Returns <see langword="null"/> when nothing is found, so callers (e.g. success messages, which
    /// must never show the generic error fallback) can decide their own safe default.
    /// </summary>
    string? TryLocalize(
        string key,
        IReadOnlyList<object>? args = null,
        CultureInfo? culture = null) => null;
}
