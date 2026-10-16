using System.Globalization;
using System.Resources;

namespace Poshteh.Core.ApplicationService.Localization;

internal sealed class ResourceManagerMessageResourceProvider : IMessageResourceProvider
{
    private readonly ResourceManager _resourceManager;

    public ResourceManagerMessageResourceProvider(Type markerType, IReadOnlyList<string>? ownerPrefixes = null)
    {
        ArgumentNullException.ThrowIfNull(markerType);

        Name = markerType.FullName ?? markerType.Name;
        _resourceManager = new ResourceManager(Name, markerType.Assembly);
        OwnerPrefixes = ownerPrefixes is { Count: > 0 } ? ownerPrefixes : Array.Empty<string>();
    }

    public string Name { get; }

    public IReadOnlyList<string> OwnerPrefixes { get; }

    public string? GetString(string key, CultureInfo culture) =>
        _resourceManager.GetString(key, culture);

    public void EnsureManifestExists() =>
        _resourceManager.GetResourceSet(CultureInfo.InvariantCulture, createIfNotExists: true, tryParents: true);
}
