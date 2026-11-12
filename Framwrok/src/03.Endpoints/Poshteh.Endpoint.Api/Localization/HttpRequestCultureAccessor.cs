using Microsoft.AspNetCore.Http;
using Poshteh.Core.ApplicationService.Localization.Json;
using Poshteh.Core.Contract.Localization;

namespace Poshteh.Endpoint.Api.Localization;

/// <summary>
/// Reads the culture <see cref="RequestCultureMiddleware"/> stored for the current request; outside an
/// HTTP request (or before the middleware ran) it returns the catalog's default culture.
/// </summary>
public sealed class HttpRequestCultureAccessor(IHttpContextAccessor httpContextAccessor, JsonMessageCatalog catalog)
    : IRequestCultureAccessor
{
    public string Culture =>
        httpContextAccessor.HttpContext?.Items[RequestCultureMiddleware.RequestCultureItemKey] is string { Length: > 0 } culture
            ? culture
            : catalog.DefaultCulture;
}
