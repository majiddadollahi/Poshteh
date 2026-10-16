using Microsoft.Extensions.Logging.Abstractions;
using Poshteh.Core.ApplicationService.Behaviors;
using Poshteh.Core.ApplicationService.Localization;
using Poshteh.Core.Domain;

namespace Poshteh.Core.ApplicationService.Tests.Behaviors;

public sealed class ExceptionHandlingBehaviorTests
{
    [Fact]
    public async Task Handle_UnexpectedException_LogsButReturnsOnlyConfiguredCode()
    {
        var options = new MessageLocalizationOptions
        {
            UnexpectedErrorCode = "Common.UnexpectedError"
        };
        var behavior = new ExceptionHandlingBehavior<string, string>(
            NullLogger<ExceptionHandlingBehavior<string, string>>.Instance,
            options);

        var result = await behavior.Handle(
            "request",
            () => throw new InvalidOperationException("sensitive database detail"));

        Assert.True(result.IsFailure);
        Assert.Equal("Common.UnexpectedError", result.Error.Code);
        Assert.Equal("Common.UnexpectedError", result.Error.Message);
        Assert.DoesNotContain("sensitive", result.Error.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Handle_DomainException_NeverCarriesTheExceptionMessageAsExplicitText()
    {
        var behavior = new ExceptionHandlingBehavior<string, string>(
            NullLogger<ExceptionHandlingBehavior<string, string>>.Instance,
            new MessageLocalizationOptions());

        var result = await behavior.Handle(
            "request",
            () => throw new DomainException("internal invariant: row 42 referenced a deleted tenant"));

        Assert.True(result.IsFailure);
        Assert.Equal("DomainException", result.Error.Code);
        // Error.Message defaults to Code when no explicit message is supplied, and the localizer
        // treats "message == code" as unsafe to surface, so this must never leak the exception text.
        Assert.Equal("DomainException", result.Error.Message);
        Assert.DoesNotContain("tenant", result.Error.Message, StringComparison.OrdinalIgnoreCase);
    }
}
