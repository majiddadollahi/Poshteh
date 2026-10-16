using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Poshteh.Core.ApplicationService.DependencyInjection;
using Poshteh.Core.ApplicationService.Localization;
using Poshteh.Core.Contract.Localization;
using Poshteh.Endpoint.Api.ExceptionHandling;

namespace Poshteh.Core.ApplicationService.Tests.ExceptionHandling;

public sealed class GlobalExceptionHandlerTests
{
    [Fact]
    public async Task TryHandleAsync_ReturnsLocalizedGenericProblemWithoutExceptionDetails()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddProblemDetails();
        services.AddPoshtehLocalization(options =>
        {
            options.FallbackKey = "Common.UnexpectedError";
            options.FallbackMessage = "پیام امن";
            options.UnexpectedErrorCode = "Common.UnexpectedError";
        });
        var provider = services.BuildServiceProvider();
        var handler = new GlobalExceptionHandler(
            provider.GetRequiredService<ILogger<GlobalExceptionHandler>>(),
            provider.GetRequiredService<IMessageLocalizer>(),
            provider.GetRequiredService<MessageLocalizationOptions>());
        var context = new DefaultHttpContext
        {
            RequestServices = provider
        };
        context.Response.Body = new MemoryStream();

        var handled = await handler.TryHandleAsync(
            context,
            new InvalidOperationException("sensitive database detail"),
            CancellationToken.None);

        context.Response.Body.Position = 0;
        var body = await new StreamReader(context.Response.Body, Encoding.UTF8).ReadToEndAsync();

        Assert.True(handled);
        Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);
        Assert.Contains("Common.UnexpectedError", body, StringComparison.Ordinal);
        Assert.Contains("پیام امن", body, StringComparison.Ordinal);
        Assert.DoesNotContain("sensitive", body, StringComparison.OrdinalIgnoreCase);
    }
}
