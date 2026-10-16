using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.DependencyInjection;
using Poshteh.Core.ApplicationService.Localization;

namespace Poshteh.Endpoint.Api.Localization;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UsePoshtehRequestLocalization(this IApplicationBuilder app)
    {
        // Forces eager construction of the message-resource registry (and therefore its
        // duplicate-provider/duplicate-prefix/manifest validation) at startup, so a misconfigured
        // resource marker fails the app before it serves a single request instead of on first lookup.
        app.ApplicationServices.GetRequiredService<MessageResourceRegistry>();

        var settings = app.ApplicationServices.GetRequiredService<MessageLocalizationOptions>();
        var defaultCulture = settings.GetDefaultCulture();
        var cultures = settings.SupportedCultureNames
            .Select(name => TryGetCulture(name))
            .Where(culture => culture is not null)
            .Cast<System.Globalization.CultureInfo>()
            .DistinctBy(culture => culture.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (cultures.All(culture =>
                !string.Equals(culture.Name, defaultCulture.Name, StringComparison.OrdinalIgnoreCase)))
        {
            cultures.Add(defaultCulture);
        }

        return app.UseRequestLocalization(new RequestLocalizationOptions
        {
            DefaultRequestCulture = new RequestCulture(defaultCulture),
            SupportedCultures = cultures,
            SupportedUICultures = cultures
        });
    }

    private static System.Globalization.CultureInfo? TryGetCulture(string name)
    {
        try
        {
            return System.Globalization.CultureInfo.GetCultureInfo(name);
        }
        catch (System.Globalization.CultureNotFoundException)
        {
            return null;
        }
    }
}
