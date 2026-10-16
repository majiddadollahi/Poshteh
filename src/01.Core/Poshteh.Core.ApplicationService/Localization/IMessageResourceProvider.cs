using System.Globalization;

namespace Poshteh.Core.ApplicationService.Localization;

internal interface IMessageResourceProvider
{
    string Name { get; }

    /// <summary>
    /// Message-code prefixes this provider owns (e.g. "Identity."). Empty means the provider is
    /// checked for every key (legacy/catch-all registration), matching the historical behavior.
    /// </summary>
    IReadOnlyList<string> OwnerPrefixes { get; }

    string? GetString(string key, CultureInfo culture);

    /// <summary>Forces the underlying resource manifest to load, throwing if it is missing/invalid.</summary>
    void EnsureManifestExists();
}
