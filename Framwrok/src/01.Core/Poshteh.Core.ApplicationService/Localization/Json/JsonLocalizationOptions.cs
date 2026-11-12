namespace Poshteh.Core.ApplicationService.Localization.Json;

/// <summary>
/// Settings for the JSON-file message catalog, bound from the <c>"Localization"</c> configuration section.
/// </summary>
public sealed class JsonLocalizationOptions
{
    public const string SectionName = "Localization";

    /// <summary>Folder holding <c>{culture}.json</c> files; relative to the content root, or absolute.</summary>
    public string ResourcesPath { get; set; } = "Localization";

    public string DefaultCulture { get; set; } = "fa";

    /// <summary>Cultures whose <c>{culture}.json</c> file must exist. Other <c>*.json</c> files are loaded too.</summary>
    public string[] SupportedCultures { get; set; } = ["fa", "en"];
}
