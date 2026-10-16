using System.Resources;

namespace Poshteh.Core.ApplicationService.Localization;

/// <summary>
/// Validates message-resource provider registration once at startup (duplicate providers, duplicate
/// owner prefixes, invalid/missing manifests) and gives the localizer a fast, prefix-scoped lookup so
/// a key lookup only checks the providers that can possibly own it instead of every registered provider.
/// </summary>
public sealed class MessageResourceRegistry
{
    private readonly IReadOnlyList<IMessageResourceProvider> _catchAllProviders;
    private readonly IReadOnlyList<IMessageResourceProvider> _prefixedProviders;

    internal MessageResourceRegistry(IEnumerable<IMessageResourceProvider> providers)
    {
        var all = providers.ToArray();

        ValidateNoDuplicateProviders(all);
        ValidateNoDuplicateOwnerPrefixes(all);
        ValidateManifests(all);

        _catchAllProviders = all.Where(provider => provider.OwnerPrefixes.Count == 0).ToArray();
        _prefixedProviders = all.Where(provider => provider.OwnerPrefixes.Count > 0).ToArray();
    }

    internal IReadOnlyList<IMessageResourceProvider> GetProviders(string key)
    {
        if (_prefixedProviders.Count == 0)
        {
            return _catchAllProviders;
        }

        List<IMessageResourceProvider>? matches = null;
        foreach (var provider in _prefixedProviders)
        {
            if (!OwnsKey(provider, key))
            {
                continue;
            }

            matches ??= new List<IMessageResourceProvider>(_catchAllProviders);
            matches.Add(provider);
        }

        return matches ?? _catchAllProviders;
    }

    private static bool OwnsKey(IMessageResourceProvider provider, string key)
    {
        foreach (var prefix in provider.OwnerPrefixes)
        {
            if (key.StartsWith(prefix, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    private static void ValidateNoDuplicateProviders(IReadOnlyList<IMessageResourceProvider> providers)
    {
        var duplicate = providers
            .GroupBy(provider => provider.Name, StringComparer.Ordinal)
            .FirstOrDefault(group => group.Count() > 1);

        if (duplicate is not null)
        {
            throw new InvalidOperationException(
                $"Message resource provider '{duplicate.Key}' is registered more than once.");
        }
    }

    private static void ValidateNoDuplicateOwnerPrefixes(IReadOnlyList<IMessageResourceProvider> providers)
    {
        var owners = new Dictionary<string, string>(StringComparer.Ordinal);

        foreach (var provider in providers)
        {
            foreach (var prefix in provider.OwnerPrefixes)
            {
                if (owners.TryGetValue(prefix, out var existingOwner))
                {
                    throw new InvalidOperationException(
                        $"Message resource prefix '{prefix}' is owned by both '{existingOwner}' and '{provider.Name}'.");
                }

                owners[prefix] = provider.Name;
            }
        }
    }

    private static void ValidateManifests(IReadOnlyList<IMessageResourceProvider> providers)
    {
        foreach (var provider in providers)
        {
            try
            {
                provider.EnsureManifestExists();
            }
            catch (MissingManifestResourceException exception)
            {
                throw new InvalidOperationException(
                    $"Message resource provider '{provider.Name}' has no valid resource manifest. " +
                    "Verify the marker type's namespace/assembly matches the embedded .resx base name.",
                    exception);
            }
        }
    }
}
