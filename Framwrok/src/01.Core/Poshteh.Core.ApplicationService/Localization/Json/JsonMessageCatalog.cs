using System.Collections.Frozen;
using System.Text.Json;

namespace Poshteh.Core.ApplicationService.Localization.Json;

/// <summary>
/// Immutable <c>culture → (code → message)</c> catalog, loaded once at startup from flat JSON files
/// (<c>Localization/fa.json</c>, <c>Localization/en.json</c>, …). Culture names are normalized to their
/// lower-case primary tag (<c>fa-IR</c> → <c>fa</c>) and compared case-insensitively; message codes are
/// case-sensitive (ordinal).
/// </summary>
public sealed class JsonMessageCatalog
{
    private const string FallbackCulture = "fa";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    private readonly FrozenDictionary<string, FrozenDictionary<string, string>> _messages;

    private JsonMessageCatalog(
        IReadOnlyList<KeyValuePair<string, FrozenDictionary<string, string>>> orderedCultures,
        string defaultCulture)
    {
        _messages = orderedCultures.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);
        Cultures = orderedCultures.Select(pair => pair.Key).ToArray();

        DefaultCulture = Normalize(defaultCulture, FallbackCulture);
        if (!_messages.ContainsKey(DefaultCulture))
        {
            throw new InvalidOperationException(
                $"The default localization culture '{DefaultCulture}' has no messages. " +
                $"Loaded cultures: {string.Join(", ", Cultures)}.");
        }
    }

    public string DefaultCulture { get; }

    /// <summary>Loaded cultures: the supported cultures in configured order, then any extra files.</summary>
    public IReadOnlyList<string> Cultures { get; }

    /// <summary>
    /// Loads every <c>*.json</c> file in <see cref="JsonLocalizationOptions.ResourcesPath"/>. Throws
    /// <see cref="DirectoryNotFoundException"/> when the folder is missing, <see cref="FileNotFoundException"/>
    /// when a supported culture has no file, and <see cref="InvalidOperationException"/> when the default
    /// culture is not loaded.
    /// </summary>
    public static JsonMessageCatalog Load(JsonLocalizationOptions options, string contentRootPath)
    {
        ArgumentNullException.ThrowIfNull(options);

        var directory = Path.IsPathRooted(options.ResourcesPath)
            ? options.ResourcesPath
            : Path.Combine(contentRootPath ?? string.Empty, options.ResourcesPath);

        if (!Directory.Exists(directory))
        {
            throw new DirectoryNotFoundException($"Localization folder '{directory}' was not found.");
        }

        var ordered = new List<KeyValuePair<string, FrozenDictionary<string, string>>>();
        var loaded = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var culture in (options.SupportedCultures ?? []).Select(name => Normalize(name, FallbackCulture)))
        {
            if (!loaded.Add(culture))
            {
                continue;
            }

            var file = Path.Combine(directory, culture + ".json");
            if (!File.Exists(file))
            {
                throw new FileNotFoundException(
                    $"Localization file for supported culture '{culture}' was not found.", file);
            }

            ordered.Add(new(culture, ReadFile(file)));
        }

        // Any other *.json file adds a language without a code/config change.
        foreach (var file in Directory.EnumerateFiles(directory, "*.json").Order(StringComparer.OrdinalIgnoreCase))
        {
            var culture = Normalize(Path.GetFileNameWithoutExtension(file), FallbackCulture);
            if (loaded.Add(culture))
            {
                ordered.Add(new(culture, ReadFile(file)));
            }
        }

        return new JsonMessageCatalog(ordered, options.DefaultCulture);
    }

    /// <summary>Builds a catalog from in-memory messages (tests, embedded catalogs).</summary>
    public static JsonMessageCatalog FromMessages(
        IEnumerable<KeyValuePair<string, IReadOnlyDictionary<string, string>>> messagesByCulture,
        string defaultCulture = FallbackCulture)
    {
        ArgumentNullException.ThrowIfNull(messagesByCulture);

        var ordered = new List<KeyValuePair<string, FrozenDictionary<string, string>>>();
        var loaded = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var (culture, messages) in messagesByCulture)
        {
            var normalized = Normalize(culture, FallbackCulture);
            if (loaded.Add(normalized))
            {
                ordered.Add(new(normalized, messages.ToFrozenDictionary(StringComparer.Ordinal)));
            }
        }

        return new JsonMessageCatalog(ordered, defaultCulture);
    }

    /// <summary>Normalizes a culture name: empty → <see cref="DefaultCulture"/>, <c>fa-IR</c> → <c>fa</c>, lower-case.</summary>
    public string NormalizeCulture(string? culture) => Normalize(culture, DefaultCulture);

    public bool HasCulture(string? culture) =>
        !string.IsNullOrWhiteSpace(culture) && _messages.ContainsKey(culture.Trim());

    /// <summary>Lookup order: requested culture → default culture → any loaded culture that has the code.</summary>
    public bool TryGet(string? culture, string code, out string message)
    {
        message = string.Empty;
        if (string.IsNullOrEmpty(code))
        {
            return false;
        }

        var requested = NormalizeCulture(culture);
        if (TryGetExact(requested, code, out message) ||
            TryGetExact(DefaultCulture, code, out message))
        {
            return true;
        }

        foreach (var candidate in Cultures)
        {
            if (TryGetExact(candidate, code, out message))
            {
                return true;
            }
        }

        return false;
    }

    private bool TryGetExact(string culture, string code, out string message)
    {
        if (_messages.TryGetValue(culture, out var messages) && messages.TryGetValue(code, out var found))
        {
            message = found;
            return true;
        }

        message = string.Empty;
        return false;
    }

    internal static string Normalize(string? culture, string emptyFallback)
    {
        if (string.IsNullOrWhiteSpace(culture))
        {
            return emptyFallback;
        }

        var trimmed = culture.Trim();
        var separator = trimmed.IndexOfAny(['-', '_']);
        var primary = separator > 0 ? trimmed[..separator] : trimmed;
        return primary.ToLowerInvariant();
    }

    private static FrozenDictionary<string, string> ReadFile(string path)
    {
        Dictionary<string, string?>? entries;
        try
        {
            using var stream = File.OpenRead(path);
            entries = JsonSerializer.Deserialize<Dictionary<string, string?>>(stream, JsonOptions);
        }
        catch (JsonException exception)
        {
            throw new InvalidDataException(
                $"Localization file '{path}' must be a flat JSON object of \"code\": \"message\" string pairs.",
                exception);
        }

        return (entries ?? [])
            .Where(pair => pair.Value is not null)
            .ToFrozenDictionary(pair => pair.Key, pair => pair.Value!, StringComparer.Ordinal);
    }
}
