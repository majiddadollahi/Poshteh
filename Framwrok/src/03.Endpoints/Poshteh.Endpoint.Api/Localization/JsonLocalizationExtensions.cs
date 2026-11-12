using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Poshteh.Core.ApplicationService.DependencyInjection;
using Poshteh.Core.ApplicationService.Localization.Json;
using Poshteh.Core.Contract.Localization;

namespace Poshteh.Endpoint.Api.Localization;

public static class JsonLocalizationExtensions
{
    /// <summary>
    /// Registers JSON-file localization (section <c>"Localization"</c>) for a web host: the catalog and
    /// <see cref="IJsonMessageLocalizer"/>/<see cref="IMessageLocalizer"/> from
    /// <see cref="JsonLocalizationServiceCollectionExtensions.AddPoshtehJsonLocalizationCore(IServiceCollection, IConfiguration, IHostEnvironment)"/>,
    /// plus a per-request <see cref="IRequestCultureAccessor"/>. Pair it with <see cref="UsePoshtehRequestCulture"/>.
    /// </summary>
    public static IServiceCollection AddPoshtehJsonLocalization(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        services.AddHttpContextAccessor();
        services.Replace(ServiceDescriptor.Singleton<IRequestCultureAccessor, HttpRequestCultureAccessor>());
        services.AddPoshtehJsonLocalizationCore(configuration, environment);
        return services;
    }

    /// <summary>Adds <see cref="RequestCultureMiddleware"/>; place it before anything that localizes messages.</summary>
    public static IApplicationBuilder UsePoshtehRequestCulture(this IApplicationBuilder app)
    {
        // Fails here, at startup, if AddPoshtehJsonLocalization was not called.
        app.ApplicationServices.GetRequiredService<JsonMessageCatalog>();
        return app.UseMiddleware<RequestCultureMiddleware>();
    }
}
