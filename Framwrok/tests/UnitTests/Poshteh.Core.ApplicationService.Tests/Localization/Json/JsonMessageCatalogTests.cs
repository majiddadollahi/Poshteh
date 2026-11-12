using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Poshteh.Core.ApplicationService.DependencyInjection;
using Poshteh.Core.ApplicationService.Localization.Json;
using Poshteh.Core.Contract.Localization;
using Poshteh.Endpoint.Api.DependencyInjection;
using Poshteh.Endpoint.Api.ExceptionHandling;
using Poshteh.Endpoint.Api.Localization;

namespace Poshteh.Core.ApplicationService.Tests.Localization.Json;

public sealed class JsonMessageCatalogTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), "poshteh-json-l10n-" + Guid.NewGuid().ToString("N"));
    private readonly string _folder;

    public JsonMessageCatalogTests()
    {
        _folder = Path.Combine(_root, "Localization");
        Directory.CreateDirectory(_folder);
    }

    public void Dispose()
    {
        if (Directory.Exists(_root))
        {
            Directory.Delete(_root, recursive: true);
        }
    }

    private void WriteFile(string name, string json) => File.WriteAllText(Path.Combine(_folder, name), json);

    private void WriteDefaultFiles()
    {
        WriteFile("fa.json", """
            {
              // comments and trailing commas are allowed
              "Common.Saved": "ذخیره شد",
              "Common.UnexpectedError": "خطای غیرمنتظره",
            }
            """);
        WriteFile("en.json", """{ "Common.Saved": "Saved" }""");
    }

    [Fact]
    public void Load_ReadsSupportedAndExtraCultureFiles()
    {
        WriteDefaultFiles();
        WriteFile("AR.json", """{ "Common.Saved": "تم الحفظ" }""");

        var catalog = JsonMessageCatalog.Load(new JsonLocalizationOptions(), _root);

        Assert.Equal(["fa", "en", "ar"], catalog.Cultures);
        Assert.True(catalog.TryGet("ar-SA", "Common.Saved", out var arabic));
        Assert.Equal("تم الحفظ", arabic);
        Assert.True(catalog.TryGet("fa", "Common.Saved", out var persian));
        Assert.Equal("ذخیره شد", persian);
    }

    [Fact]
    public void Load_KeysAreCaseSensitive()
    {
        WriteDefaultFiles();

        var catalog = JsonMessageCatalog.Load(new JsonLocalizationOptions(), _root);

        Assert.False(catalog.TryGet("en", "common.saved", out _));
    }

    [Fact]
    public void Load_AbsoluteResourcesPath_IgnoresContentRoot()
    {
        WriteDefaultFiles();

        var catalog = JsonMessageCatalog.Load(new JsonLocalizationOptions { ResourcesPath = _folder }, "Z:\\does-not-matter");

        Assert.Equal("fa", catalog.DefaultCulture);
    }

    [Fact]
    public void Load_MissingFolder_ThrowsDirectoryNotFound()
    {
        Assert.Throws<DirectoryNotFoundException>(() =>
            JsonMessageCatalog.Load(new JsonLocalizationOptions { ResourcesPath = "Nope" }, _root));
    }

    [Fact]
    public void Load_MissingSupportedCultureFile_ThrowsFileNotFound()
    {
        WriteFile("fa.json", "{}");

        Assert.Throws<FileNotFoundException>(() => JsonMessageCatalog.Load(new JsonLocalizationOptions(), _root));
    }

    [Fact]
    public void Load_DefaultCultureNotLoaded_ThrowsInvalidOperation()
    {
        WriteDefaultFiles();

        Assert.Throws<InvalidOperationException>(() =>
            JsonMessageCatalog.Load(new JsonLocalizationOptions { DefaultCulture = "de" }, _root));
    }

    [Fact]
    public void NormalizeCulture_EmptyIsDefault_RegionIsDropped_AndLowerCased()
    {
        var catalog = TestJsonCatalog.Create();

        Assert.Equal("fa", catalog.NormalizeCulture(""));
        Assert.Equal("fa", catalog.NormalizeCulture(null));
        Assert.Equal("fa", catalog.NormalizeCulture("fa-IR"));
        Assert.Equal("en", catalog.NormalizeCulture("EN-us"));
    }

    [Fact]
    public void AddPoshtehJsonLocalization_BindsSection_AndReplacesResxLocalizer()
    {
        WriteFile("fa.json", """{ "Common.Saved": "ذخیره شد" }""");
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Localization:DefaultCulture"] = "fa",
                // Must replace (not append to) the default ["fa", "en"]; there is no en.json here.
                ["Localization:SupportedCultures:0"] = "fa"
            })
            .Build();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddEndpointApi(); // registers the resx IMessageLocalizer first
        services.AddPoshtehJsonLocalization(configuration, new TestHostEnvironment(_root));
        using var provider = services.BuildServiceProvider();

        var localizer = provider.GetRequiredService<IMessageLocalizer>();
        Assert.IsType<JsonMessageLocalizer>(localizer);
        Assert.Same(localizer, provider.GetRequiredService<IJsonMessageLocalizer>());
        Assert.IsType<HttpRequestCultureAccessor>(provider.GetRequiredService<IRequestCultureAccessor>());
        Assert.Equal("ذخیره شد", provider.GetRequiredService<IJsonMessageLocalizer>().Get("Common.Saved"));
        Assert.NotNull(ActivatorUtilities.CreateInstance<GlobalExceptionHandler>(provider));
    }

    [Fact]
    public void AddPoshtehJsonLocalizationCore_UsesDefaultCultureAccessor()
    {
        WriteDefaultFiles();

        var services = new ServiceCollection();
        services.AddPoshtehJsonLocalizationCore(new ConfigurationBuilder().Build(), new TestHostEnvironment(_root));
        using var provider = services.BuildServiceProvider();

        Assert.Equal("fa", provider.GetRequiredService<IRequestCultureAccessor>().Culture);
        Assert.Equal("Saved", provider.GetRequiredService<IJsonMessageLocalizer>().GetForCulture("en", "Common.Saved"));
    }

    private sealed class TestHostEnvironment(string contentRootPath) : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = "Test";
        public string ApplicationName { get; set; } = "Test";
        public string ContentRootPath { get; set; } = contentRootPath;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
