using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Poshteh.Core.ApplicationService.Behaviors;
using Poshteh.Core.ApplicationService.Commands;
using Poshteh.Core.ApplicationService.Events;
using Poshteh.Core.ApplicationService.Localization;
using Poshteh.Core.ApplicationService.Queries;
using Poshteh.Core.Contract;
using Poshteh.Core.Contract.Localization;
using Poshteh.Shared.Kernel.Correlation;
using Poshteh.Shared.Kernel.Time;
using System.Reflection;

namespace Poshteh.Core.ApplicationService.DependencyInjection;

/// <summary>
/// Wires the default dispatcher/behavior/event infrastructure and scans the target assemblies for handlers.
/// The default behaviors are intentionally minimal and framework-level, but they remain extension points.
/// </summary>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPoshtehLocalization(
        this IServiceCollection services,
        Action<MessageLocalizationOptions>? configure = null)
    {
        var options = new MessageLocalizationOptions();
        configure?.Invoke(options);

        services.TryAddSingleton(options);
        services.TryAddSingleton(sp => new MessageResourceRegistry(sp.GetServices<IMessageResourceProvider>()));
        services.TryAddSingleton<IMessageLocalizer, ResourceManagerMessageLocalizer>();
        return services;
    }

    /// <summary>
    /// Registers a message-resource marker and the key prefixes it owns (e.g. "Identity.").
    /// A key is only looked up in providers whose prefix it starts with, plus any provider registered
    /// with no prefix (catch-all/legacy). Two providers must not claim the same prefix; this and an
    /// invalid/missing resource manifest are detected eagerly the first time the registry is resolved
    /// (<see cref="Poshteh.Endpoint.Api.Localization.ApplicationBuilderExtensions.UsePoshtehRequestLocalization"/>
    /// forces that at app startup), not on the first request.
    /// </summary>
    public static IServiceCollection AddPoshtehMessageResources<TMarker>(
        this IServiceCollection services, params string[] ownerPrefixes)
    {
        services.AddSingleton<IMessageResourceProvider>(
            new ResourceManagerMessageResourceProvider(typeof(TMarker), ownerPrefixes));
        return services;
    }

    public static IServiceCollection AddApplicationService(this IServiceCollection services, params Assembly[] assemblies)
    {
        services.AddSingleton<IClock, SystemClock>();
        services.AddScoped<ICorrelationContextAccessor, AsyncLocalCorrelationContextAccessor>();
        services.AddScoped<ICommandDispatcher, CommandDispatcher>();
        services.AddScoped<IQueryDispatcher, QueryDispatcher>();
        services.AddScoped<IEventBus, InMemoryEventBus>();
        services.AddScoped<IDomainEventDispatcher, EventBusDomainEventDispatcher>();

        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ExceptionHandlingBehavior<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));

        RegisterOpenGenericImplementations(services, assemblies, typeof(ICommandHandler<,>));
        RegisterOpenGenericImplementations(services, assemblies, typeof(IQueryHandler<,>));
        RegisterOpenGenericImplementations(services, assemblies, typeof(IDomainEventHandler<>));
        RegisterOpenGenericImplementations(services, assemblies, typeof(IPipelineBehavior<,>));

        return services;
    }

    private static void RegisterOpenGenericImplementations(
        IServiceCollection services,
        Assembly[] assemblies,
        Type openGenericInterface)
    {
        var registrations =
            from assembly in assemblies
            from type in assembly.GetTypes()
            where type is { IsAbstract: false, IsInterface: false }
            from @interface in type.GetInterfaces()
            where @interface.IsGenericType && @interface.GetGenericTypeDefinition() == openGenericInterface
            select (Service: @interface, Implementation: type);

        foreach (var (service, implementation) in registrations)
        {
            services.AddScoped(service, implementation);
        }
    }
}
