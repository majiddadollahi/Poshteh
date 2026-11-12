using System.Globalization;
using Microsoft.AspNetCore.Http;
using Poshteh.Core.ApplicationService.Tests.Localization.Json;
using Poshteh.Endpoint.Api.Localization;

namespace Poshteh.Core.ApplicationService.Tests.Endpoint;

public sealed class RequestCultureMiddlewareTests
{
    [Theory]
    [InlineData(null, "fa")]
    [InlineData("", "fa")]
    [InlineData("fa-IR,fa;q=0.9,en-US;q=0.8", "fa")]
    [InlineData("en-US,en;q=0.9,fa;q=0.8", "en")]
    [InlineData("en;q=0.1,fa;q=0.9", "en")] // header order wins, q-values are ignored
    [InlineData("*,en", "en")]
    [InlineData("ja-JP,de-DE", "de")]
    [InlineData("ja-JP,zh", "fa")]
    [InlineData("  EN-gb  ", "en")]
    [InlineData("*", "fa")]
    public void ResolveCulture_ParsesAcceptLanguageInOrder(string? header, string expected)
    {
        Assert.Equal(expected, RequestCultureMiddleware.ResolveCulture(header, TestJsonCatalog.Create()));
    }

    [Fact]
    public async Task InvokeAsync_StoresCultureInItems_AndSetsThreadCulture()
    {
        var catalog = TestJsonCatalog.Create();
        var context = new DefaultHttpContext();
        context.Request.Headers.AcceptLanguage = "en-US,en;q=0.9";
        string? uiCultureInNext = null;
        string? cultureInNext = null;

        var previousCulture = CultureInfo.CurrentCulture;
        var previousUiCulture = CultureInfo.CurrentUICulture;
        try
        {
            var middleware = new RequestCultureMiddleware(
                _ =>
                {
                    cultureInNext = CultureInfo.CurrentCulture.Name;
                    uiCultureInNext = CultureInfo.CurrentUICulture.Name;
                    return Task.CompletedTask;
                },
                catalog);

            await middleware.InvokeAsync(context);
        }
        finally
        {
            CultureInfo.CurrentCulture = previousCulture;
            CultureInfo.CurrentUICulture = previousUiCulture;
        }

        Assert.Equal("en", context.Items[RequestCultureMiddleware.RequestCultureItemKey]);
        Assert.Equal("Poshteh.RequestCulture", RequestCultureMiddleware.RequestCultureItemKey);
        Assert.Equal("en", cultureInNext);
        Assert.Equal("en", uiCultureInNext);
    }

    [Fact]
    public void HttpRequestCultureAccessor_ReadsItems_AndFallsBackToDefaultOutsideRequests()
    {
        var catalog = TestJsonCatalog.Create();
        var httpContextAccessor = new HttpContextAccessor();
        var accessor = new HttpRequestCultureAccessor(httpContextAccessor, catalog);

        Assert.Equal("fa", accessor.Culture);

        var context = new DefaultHttpContext();
        context.Items[RequestCultureMiddleware.RequestCultureItemKey] = "en";
        httpContextAccessor.HttpContext = context;

        Assert.Equal("en", accessor.Culture);
    }
}
