using BuildingBlocks.Web.Abstractions;
using PM.Identities.EndpointApi.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace PM.Identities.EndpointApi;

public class IdentityApiModule : IApiModule
{
    public void Register(IServiceCollection services, IConfiguration configuration)
    {
        // ثبت تمام وابستگی‌های ماژول Identity
        services.AddIdentityModule(configuration);
    }

    public void MapEndpoints(WebApplication app)
    {
        // Endpoints will be mapped by controllers
    }
}

