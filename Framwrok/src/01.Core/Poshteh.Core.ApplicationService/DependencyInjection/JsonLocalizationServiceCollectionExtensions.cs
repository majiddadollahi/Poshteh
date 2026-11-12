using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Poshteh.Core.ApplicationService.Localization;
using Poshteh.Core.ApplicationService.Localization.Json;
using Poshteh.Core.Contract.Localization;

namespace Poshteh.Core.ApplicationService.DependencyInjection;

public static class JsonLocalizationServiceCollectionExtensions
{
    /// <summary>
    /// Registers the JSON-file message catalog and <see cref="JsonMessageLocalizer"/> as both
    /// <see cref="IJsonMessageLocalizer"/> and <see cref="IMessageLocalizer"/> (replacing the resx localizer if
    /// one was registered), with a culture accessor that always returns the default culture.
    /// The catalog is loaded and validated immediately, so a missing folder/file fails at startup.
    /// <para>Web hosts call <c>AddPoshtehJsonLocalization</c> from Poshteh.Endpoint.Api instead, which adds
    /// the per-request culture on top of this.</para>
    /// </summary>
    public static IServiceCollection AddPoshtehJsonLocalizationCore(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(environment);

        return services.AddPoshtehJsonLocalizationCore(
            BindOptions(configuration.GetSection(JsonLocalizationOptions.SectionName)),
            environment.ContentRootPath);
    }

    public static IServiceCollection AddPoshtehJsonLocalizationCore(
        this IServiceCollection services,
        JsonLocalizationOptions options,
        string contentRootPath)
    {
        ArgumentNullException.ThrowIfNull(options);

        var catalog = JsonMessageCatalog.Load(options, contentRootPath);

        services.TryAddSingleton(options);
        services.TryAddSingleton(catalog);
        services.TryAddSingleton<IRequestCultureAccessor, DefaultRequestCultureAccessor>();

        // GlobalExceptionHandler/ExceptionHandlingBehavior read the unexpected-error code from here.
        var fallbackOptions = new MessageLocalizationOptions { DefaultCultureName = catalog.DefaultCulture };
        foreach (var culture in catalog.Cultures)
        {
            fallbackOptions.SupportedCultureNames.Add(culture);
        }

        services.TryAddSingleton(fallbackOptions);
        services.TryAddSingleton<JsonMessageLocalizer>();
        services.TryAddSingleton<IJsonMessageLocalizer>(sp => sp.GetRequiredService<JsonMessageLocalizer>());
        services.Replace(ServiceDescriptor.Singleton<IMessageLocalizer>(sp => sp.GetRequiredService<JsonMessageLocalizer>()));

        return services;
    }

    // Bound by hand: ConfigurationBinder appends configured array items to the default
    // SupportedCultures instead of replacing them.
    private static JsonLocalizationOptions BindOptions(IConfigurationSection section)
    {
        var options = new JsonLocalizationOptions();

        if (!string.IsNullOrWhiteSpace(section[nameof(JsonLocalizationOptions.ResourcesPath)]))
        {
            options.ResourcesPath = section[nameof(JsonLocalizationOptions.ResourcesPath)]!;
        }

        if (!string.IsNullOrWhiteSpace(section[nameof(JsonLocalizationOptions.DefaultCulture)]))
        {
            options.DefaultCulture = section[nameof(JsonLocalizationOptions.DefaultCulture)]!;
        }

        var cultures = section.GetSection(nameof(JsonLocalizationOptions.SupportedCultures))
            .GetChildren()
            .Select(child => child.Value)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value!)
            .ToArray();
        if (cultures.Length > 0)
        {
            options.SupportedCultures = cultures;
        }

        return options;
    }
}
