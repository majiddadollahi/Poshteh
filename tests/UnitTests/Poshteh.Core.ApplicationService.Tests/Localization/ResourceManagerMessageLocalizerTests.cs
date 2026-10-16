using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Poshteh.Core.ApplicationService.DependencyInjection;
using Poshteh.Core.ApplicationService.Localization;
using Poshteh.Core.Contract.Localization;
using Poshteh.Core.Domain;

namespace Poshteh.Core.ApplicationService.Tests.Localization;

public sealed class ResourceManagerMessageLocalizerTests
{
    [Fact]
    public void Localize_UsesRequestedCultureAndFormatsArguments()
    {
        var localizer = CreateLocalizer();

        var greeting = localizer.Localize("Greeting", culture: CultureInfo.GetCultureInfo("fa-IR"));
        var parameterized = localizer.Localize(
            new Error("Parameterized", args: 25),
            CultureInfo.GetCultureInfo("fa-IR"));

        Assert.Equal("سلام", greeting);
        Assert.Equal("حداکثر طول 25 نویسه است.", parameterized);
    }

    [Fact]
    public void Localize_FallsBackToDefaultCultureThenSafeMessage()
    {
        var localizer = CreateLocalizer();

        Assert.Equal(
            "پیام پیش‌فرض",
            localizer.Localize("DefaultOnly", culture: CultureInfo.GetCultureInfo("en-US")));
        Assert.Equal(
            "پیام امن",
            localizer.Localize("Missing.Key", culture: CultureInfo.GetCultureInfo("fa-IR")));
        Assert.Equal(
            "پیام امن",
            localizer.Localize("InvalidFormat", ["one"], culture: CultureInfo.GetCultureInfo("fa-IR")));
    }

    [Fact]
    public void Localize_ResolvesMultipleProvidersAndUsesDeterministicFirstProviderForCollision()
    {
        var localizer = CreateLocalizer();

        var culture = CultureInfo.GetCultureInfo("en-US");
        Assert.Equal("Second provider", localizer.Localize("Additional", culture: culture));
        Assert.Equal("Hello", localizer.Localize("Greeting", culture: culture));
    }

    [Fact]
    public void Localize_DoesNotExposeRawKeysFromExplicitMessages()
    {
        var localizer = CreateLocalizer();

        var message = localizer.Localize(
            new Error("Wrapper.Code", "Nested.RawKey"),
            CultureInfo.GetCultureInfo("fa-IR"));

        Assert.Equal("پیام امن", message);
    }

    [Fact]
    public void Localize_WithOwnerPrefixes_OnlyResolvesTheOwningProvider()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddPoshtehLocalization();
        services.AddPoshtehMessageResources<OwnedMessages>("Owned.");
        services.AddPoshtehMessageResources<OtherOwnedMessages>("Other.");

        var localizer = services.BuildServiceProvider().GetRequiredService<IMessageLocalizer>();

        // OtherOwnedMessages also defines "Owned.Key" with a different value, but it does not own the
        // "Owned." prefix, so it must never be consulted for this key.
        Assert.Equal("Owned value", localizer.Localize("Owned.Key"));
        Assert.Equal("Other value", localizer.Localize("Other.Key"));
    }

    [Fact]
    public void AddPoshtehMessageResources_SameMarkerRegisteredTwice_FailsFastOnRegistryResolution()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddPoshtehLocalization();
        services.AddPoshtehMessageResources<TestMessages>();
        services.AddPoshtehMessageResources<TestMessages>();

        var provider = services.BuildServiceProvider();

        Assert.Throws<InvalidOperationException>(() => provider.GetRequiredService<MessageResourceRegistry>());
    }

    [Fact]
    public void AddPoshtehMessageResources_TwoMarkersClaimTheSamePrefix_FailsFastOnRegistryResolution()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddPoshtehLocalization();
        services.AddPoshtehMessageResources<OwnedMessages>("Shared.");
        services.AddPoshtehMessageResources<OtherOwnedMessages>("Shared.");

        var provider = services.BuildServiceProvider();

        var exception = Assert.Throws<InvalidOperationException>(
            () => provider.GetRequiredService<MessageResourceRegistry>());
        Assert.Contains("Shared.", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AddPoshtehMessageResources_MarkerWithoutMatchingResx_FailsFastOnRegistryResolution()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddPoshtehLocalization();
        services.AddPoshtehMessageResources<NoManifestMessages>();

        var provider = services.BuildServiceProvider();

        Assert.Throws<InvalidOperationException>(() => provider.GetRequiredService<MessageResourceRegistry>());
    }

    [Fact]
    public void TryLocalize_MissingKey_ReturnsNullInsteadOfTheUnexpectedErrorFallback()
    {
        var localizer = CreateLocalizer();

        Assert.Null(localizer.TryLocalize("Missing.Key", culture: CultureInfo.GetCultureInfo("fa-IR")));
    }

    [Fact]
    public void TryLocalize_ExistingKey_ReturnsTheResolvedText()
    {
        var localizer = CreateLocalizer();

        Assert.Equal(
            "سلام",
            localizer.TryLocalize("Greeting", culture: CultureInfo.GetCultureInfo("fa-IR")));
    }

    [Fact]
    public void Localize_MissingKeyRequestedRepeatedly_LogsTheWarningOnlyOnce()
    {
        var services = new ServiceCollection();
        var capturingProvider = new CapturingLoggerProvider();
        services.AddLogging(builder => builder.AddProvider(capturingProvider));
        services.AddPoshtehLocalization(options =>
        {
            options.FallbackKey = "Common.Fallback";
            options.FallbackMessage = "Last-resort fallback";
        });
        services.AddPoshtehMessageResources<TestMessages>();
        var localizer = services.BuildServiceProvider().GetRequiredService<IMessageLocalizer>();

        localizer.Localize("Still.Missing");
        localizer.Localize("Still.Missing");
        localizer.Localize("Still.Missing");

        Assert.Equal(1, capturingProvider.WarningCount);
    }

    private sealed class CapturingLoggerProvider : ILoggerProvider
    {
        private int _warningCount;

        public int WarningCount => _warningCount;

        public ILogger CreateLogger(string categoryName) => new CapturingLogger(this);

        public void Dispose()
        {
        }

        private sealed class CapturingLogger(CapturingLoggerProvider owner) : ILogger
        {
            public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(
                LogLevel logLevel,
                EventId eventId,
                TState state,
                Exception? exception,
                Func<TState, Exception?, string> formatter)
            {
                if (logLevel == LogLevel.Warning)
                {
                    Interlocked.Increment(ref owner._warningCount);
                }
            }
        }
    }

    private static IMessageLocalizer CreateLocalizer()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddPoshtehLocalization(options =>
        {
            options.DefaultCultureName = "fa-IR";
            options.SupportedCultureNames.Add("fa-IR");
            options.SupportedCultureNames.Add("en-US");
            options.FallbackKey = "Common.Fallback";
            options.FallbackMessage = "Last-resort fallback";
        });
        services.AddPoshtehMessageResources<TestMessages>();
        services.AddPoshtehMessageResources<AdditionalMessages>();

        return services.BuildServiceProvider().GetRequiredService<IMessageLocalizer>();
    }
}
