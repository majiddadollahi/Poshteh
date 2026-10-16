using System.Collections.Concurrent;
using System.Globalization;
using System.Resources;
using Microsoft.Extensions.Logging;
using Poshteh.Core.Contract.Localization;
using Poshteh.Core.Domain;

namespace Poshteh.Core.ApplicationService.Localization;

internal sealed class ResourceManagerMessageLocalizer : IMessageLocalizer
{
    private readonly MessageResourceRegistry _registry;
    private readonly MessageLocalizationOptions _options;
    private readonly ILogger<ResourceManagerMessageLocalizer> _logger;
    private readonly ConcurrentDictionary<string, byte> _loggedOnce = new(StringComparer.Ordinal);

    public ResourceManagerMessageLocalizer(
        MessageResourceRegistry registry,
        MessageLocalizationOptions options,
        ILogger<ResourceManagerMessageLocalizer> logger)
    {
        _registry = registry;
        _options = options;
        _logger = logger;
    }

    public string? TryLocalize(
        string key,
        IReadOnlyList<object>? args = null,
        CultureInfo? culture = null)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return null;
        }

        var requestedCulture = ResolveRequestedCulture(culture ?? CultureInfo.CurrentUICulture);
        var localized = FindResource(key, requestedCulture);
        if (localized is null)
        {
            return null;
        }

        if (args is not { Count: > 0 })
        {
            return localized;
        }

        try
        {
            return string.Format(requestedCulture, localized, args.ToArray());
        }
        catch (FormatException)
        {
            return null;
        }
    }

    public string Localize(Error error, CultureInfo? culture = null)
    {
        ArgumentNullException.ThrowIfNull(error);
        return Localize(error.Code, error.Args, error.Message, culture);
    }

    public string Localize(
        string key,
        IReadOnlyList<object>? args = null,
        string? explicitMessage = null,
        CultureInfo? culture = null)
    {
        var requestedCulture = ResolveRequestedCulture(culture ?? CultureInfo.CurrentUICulture);

        var localized = FindResource(key, requestedCulture);
        if (localized is null && IsSafeExplicitMessage(key, explicitMessage))
        {
            localized = explicitMessage;
        }

        localized ??= FindResource(_options.FallbackKey, requestedCulture);
        localized ??= _options.FallbackMessage;

        if (args is not { Count: > 0 })
        {
            return localized;
        }

        try
        {
            return string.Format(requestedCulture, localized, args.ToArray());
        }
        catch (FormatException exception)
        {
            _logger.LogWarning(exception, "Invalid localized message format for key {MessageKey}", key);
            return FindResource(_options.FallbackKey, requestedCulture) ?? _options.FallbackMessage;
        }
    }

    private CultureInfo ResolveRequestedCulture(CultureInfo culture)
    {
        if (_options.SupportedCultureNames.Count == 0 ||
            _options.SupportedCultureNames.Contains(culture.Name, StringComparer.OrdinalIgnoreCase))
        {
            return culture;
        }

        return _options.GetDefaultCulture();
    }

    private string? FindResource(string key, CultureInfo requestedCulture)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return null;
        }

        var providers = _registry.GetProviders(key);

        foreach (var culture in EnumerateCultures(requestedCulture))
        {
            string? resolved = null;
            string? winningProvider = null;

            foreach (var provider in providers)
            {
                string? candidate;
                try
                {
                    candidate = provider.GetString(key, culture);
                }
                catch (MissingManifestResourceException exception)
                {
                    LogOnce(
                        $"manifest|{provider.Name}",
                        () => _logger.LogWarning(
                            exception,
                            "Message resource provider {ProviderName} is incomplete",
                            provider.Name));
                    continue;
                }
                catch (MissingSatelliteAssemblyException exception)
                {
                    LogOnce(
                        $"satellite|{provider.Name}|{culture.Name}",
                        () => _logger.LogWarning(
                            exception,
                            "Message satellite resource for provider {ProviderName} and culture {Culture} is missing",
                            provider.Name,
                            culture.Name));
                    continue;
                }

                if (string.IsNullOrWhiteSpace(candidate))
                {
                    continue;
                }

                if (resolved is null)
                {
                    resolved = candidate;
                    winningProvider = provider.Name;
                    continue;
                }

                LogOnce(
                    $"duplicate|{key}|{culture.Name}",
                    () => _logger.LogWarning(
                        "Duplicate message key {MessageKey} for culture {Culture}; provider {WinningProvider} wins over {DuplicateProvider}",
                        key,
                        culture.Name,
                        winningProvider,
                        provider.Name));
            }

            if (resolved is not null)
            {
                return resolved;
            }
        }

        LogOnce(
            $"missing|{key}|{requestedCulture.Name}",
            () => _logger.LogWarning("No localized message resource was found for key {MessageKey}", key));
        return null;
    }

    private void LogOnce(string dedupeKey, Action logAction)
    {
        if (_loggedOnce.TryAdd(dedupeKey, 0))
        {
            logAction();
        }
    }

    private IEnumerable<CultureInfo> EnumerateCultures(CultureInfo requestedCulture)
    {
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var current = requestedCulture;

        while (seen.Add(current.Name))
        {
            yield return current;
            if (current.Equals(CultureInfo.InvariantCulture))
            {
                break;
            }

            current = current.Parent;
        }

        current = _options.GetDefaultCulture();
        while (seen.Add(current.Name))
        {
            yield return current;
            if (current.Equals(CultureInfo.InvariantCulture))
            {
                break;
            }

            current = current.Parent;
        }
    }

    private static bool IsSafeExplicitMessage(string key, string? message) =>
        !string.IsNullOrWhiteSpace(message) &&
        !string.Equals(key, message, StringComparison.Ordinal) &&
        !LooksLikeMessageKey(message);

    private static bool LooksLikeMessageKey(string value) =>
        value.Contains('.', StringComparison.Ordinal) &&
        !value.Any(char.IsWhiteSpace) &&
        value.All(character => char.IsLetterOrDigit(character) || character is '.' or '_' or '-');
}
