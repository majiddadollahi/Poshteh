using System.Globalization;
using Poshteh.Core.Contract.Localization;
using Poshteh.Core.Domain;

namespace Poshteh.Core.ApplicationService.Localization.Json;

/// <summary>
/// <see cref="IMessageLocalizer"/> backed by a <see cref="JsonMessageCatalog"/>.
/// <para>
/// The <see cref="IJsonMessageLocalizer"/> <c>Get</c> overloads keep the "missing translation returns the code"
/// contract (<c>code|arg1|arg2</c> with arguments). The <see cref="IMessageLocalizer"/> members keep Poshteh's
/// safe contract instead (missing key → safe explicit message → <see cref="MessageLocalizationOptions.FallbackKey"/>
/// → <see cref="MessageLocalizationOptions.FallbackMessage"/>), so <c>ToApiResult</c> and
/// <c>GlobalExceptionHandler</c> behave exactly as with the resx localizer.
/// </para>
/// Arguments are always formatted with <see cref="CultureInfo.InvariantCulture"/>; a template whose
/// placeholders do not match its arguments is returned unformatted.
/// </summary>
public sealed class JsonMessageLocalizer : IJsonMessageLocalizer
{
    private const string PersianListSeparator = "؛ ";
    private const string ListSeparator = "; ";

    private readonly JsonMessageCatalog _catalog;
    private readonly IRequestCultureAccessor _cultureAccessor;
    private readonly MessageLocalizationOptions _fallbackOptions;

    public JsonMessageLocalizer(
        JsonMessageCatalog catalog,
        IRequestCultureAccessor cultureAccessor,
        MessageLocalizationOptions? fallbackOptions = null)
    {
        _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
        _cultureAccessor = cultureAccessor ?? throw new ArgumentNullException(nameof(cultureAccessor));
        _fallbackOptions = fallbackOptions ?? new MessageLocalizationOptions();
    }

    private string RequestCulture => _catalog.NormalizeCulture(_cultureAccessor.Culture);

    public string Get(string code) => GetForCulture(RequestCulture, code);

    public string Get(string code, params object[] args) => GetForCulture(RequestCulture, code, args);

    public string Get(Error error)
    {
        ArgumentNullException.ThrowIfNull(error);
        return GetError(RequestCulture, error);
    }

    public string GetForCulture(string culture, string code, params object[] args)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return string.Empty;
        }

        args ??= [];
        if (!_catalog.TryGet(culture, code, out var template))
        {
            return args.Length == 0 ? code : code + "|" + string.Join("|", args);
        }

        return Format(template, args);
    }

    public string Localize(
        string key,
        IReadOnlyList<object>? args = null,
        string? explicitMessage = null,
        CultureInfo? culture = null)
    {
        var cultureName = ResolveCulture(culture);
        var formatArgs = args is { Count: > 0 } ? args.ToArray() : [];

        if (!string.IsNullOrWhiteSpace(key) && _catalog.TryGet(cultureName, key, out var template))
        {
            return Format(template, formatArgs);
        }

        return Fallback(key, explicitMessage, formatArgs, cultureName);
    }

    public string Localize(Error error, CultureInfo? culture = null)
    {
        ArgumentNullException.ThrowIfNull(error);

        var cultureName = ResolveCulture(culture);
        if (!string.IsNullOrWhiteSpace(error.Code) && _catalog.TryGet(cultureName, error.Code, out _))
        {
            return GetError(cultureName, error);
        }

        return Fallback(error.Code, error.Message, error.Args, cultureName);
    }

    public string? TryLocalize(string key, IReadOnlyList<object>? args = null, CultureInfo? culture = null)
    {
        if (string.IsNullOrWhiteSpace(key) || !_catalog.TryGet(ResolveCulture(culture), key, out var template))
        {
            return null;
        }

        return Format(template, args is { Count: > 0 } ? args.ToArray() : []);
    }

    private string GetError(string culture, Error error)
    {
        var args = error.Args;
        if (args.Length == 0)
        {
            return GetForCulture(culture, error.Code);
        }

        if (!args.All(arg => arg is string))
        {
            return GetForCulture(culture, error.Code, args);
        }

        // Every argument is itself a message code (e.g. the individual password rules behind
        // Identity.PasswordInvalid): localize each one and pass them on as a single {0} argument.
        var parts = args
            .Cast<string>()
            .Where(part => !string.IsNullOrWhiteSpace(part))
            .Select(part => _catalog.TryGet(culture, part, out var localized) ? localized : part)
            .ToArray();

        return parts.Length switch
        {
            0 => GetForCulture(culture, error.Code),
            1 => GetForCulture(culture, error.Code, parts[0]),
            _ => GetForCulture(culture, error.Code, string.Join(SeparatorFor(culture), parts))
        };
    }

    private string Fallback(string key, string? explicitMessage, object[] args, string culture)
    {
        if (ExplicitMessageSafety.IsSafe(key, explicitMessage))
        {
            return Format(explicitMessage!, args);
        }

        return _catalog.TryGet(culture, _fallbackOptions.FallbackKey, out var fallback)
            ? fallback
            : _fallbackOptions.FallbackMessage;
    }

    private string ResolveCulture(CultureInfo? culture) =>
        culture is null ? RequestCulture : _catalog.NormalizeCulture(culture.Name);

    private static string SeparatorFor(string culture) =>
        culture.StartsWith("fa", StringComparison.OrdinalIgnoreCase) ? PersianListSeparator : ListSeparator;

    private static string Format(string template, object[] args)
    {
        if (args.Length == 0)
        {
            return template;
        }

        try
        {
            return string.Format(CultureInfo.InvariantCulture, template, args);
        }
        catch (FormatException)
        {
            return template;
        }
    }
}
