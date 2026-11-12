using System.Globalization;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using Poshteh.Core.ApplicationService.Localization.Json;

namespace Poshteh.Endpoint.Api.Localization;

/// <summary>
/// Resolves the request culture once per request from <c>Accept-Language</c>, stores it in
/// <see cref="HttpContext.Items"/> under <see cref="RequestCultureItemKey"/> and sets
/// <see cref="CultureInfo.CurrentCulture"/>/<see cref="CultureInfo.CurrentUICulture"/>.
/// </summary>
public sealed class RequestCultureMiddleware(RequestDelegate next, JsonMessageCatalog catalog)
{
    public const string RequestCultureItemKey = "Poshteh.RequestCulture";

    public Task InvokeAsync(HttpContext context)
    {
        var culture = ResolveCulture(context.Request.Headers[HeaderNames.AcceptLanguage].ToString(), catalog);
        context.Items[RequestCultureItemKey] = culture;

        try
        {
            var cultureInfo = CultureInfo.GetCultureInfo(culture);
            CultureInfo.CurrentCulture = cultureInfo;
            CultureInfo.CurrentUICulture = cultureInfo;
        }
        catch (CultureNotFoundException)
        {
            // A catalog file named after a culture .NET does not know still localizes messages;
            // only the thread culture is left untouched.
        }

        return next(context);
    }

    /// <summary>
    /// Walks the <c>Accept-Language</c> tags in header order (q-values are ignored, <c>*</c> is skipped).
    /// For each tag the primary subtag (<c>fa</c> of <c>fa-IR</c>) wins if loaded, else the full tag;
    /// with no match the catalog's default culture is used.
    /// </summary>
    public static string ResolveCulture(string? acceptLanguage, JsonMessageCatalog catalog)
    {
        ArgumentNullException.ThrowIfNull(catalog);

        if (string.IsNullOrWhiteSpace(acceptLanguage))
        {
            return catalog.DefaultCulture;
        }

        foreach (var entry in acceptLanguage.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var tag = entry.Split(';', 2)[0].Trim();
            if (tag.Length == 0 || tag == "*")
            {
                continue;
            }

            var separator = tag.IndexOf('-', StringComparison.Ordinal);
            var primary = separator > 0 ? tag[..separator] : tag;
            if (catalog.HasCulture(primary))
            {
                return primary.ToLowerInvariant();
            }

            if (catalog.HasCulture(tag))
            {
                return tag.ToLowerInvariant();
            }
        }

        return catalog.DefaultCulture;
    }
}
