using Microsoft.Extensions.DependencyInjection;
using Poshteh.Core.ApplicationService.DependencyInjection;
using Poshteh.Endpoint.Api.ExceptionHandling;

namespace Poshteh.Endpoint.Api.DependencyInjection;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers MVC controllers (so <c>BaseController</c>-derived types are discovered) and the central
    /// exception-handling seam (<see cref="GlobalExceptionHandler"/> + ProblemDetails).
    /// Callers still need <c>app.MapControllers()</c> and <c>app.UseExceptionHandler()</c> in the request
    /// pipeline for this to take effect.
    /// </summary>
    public static IServiceCollection AddEndpointApi(this IServiceCollection services)
    {
        services.AddPoshtehLocalization();
        services.AddControllers();
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();

        return services;
    }
}
