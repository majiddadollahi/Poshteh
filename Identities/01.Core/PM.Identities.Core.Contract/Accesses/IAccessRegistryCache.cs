using PM.Identities.Core.Contract.Accesses.DTOs;

namespace PM.Identities.Core.Contract.Accesses;

/// <summary>
/// TASK-0247: immutable in-memory copy of the endpoint Access registry used on every request.
/// <see cref="Endpoints"/> is keyed by <see cref="IdentityOf"/>; <see cref="FeatureKeysByAccess"/>
/// holds the normalized keys of enabled Features linked to each Access.
/// </summary>
public sealed class AccessRegistrySnapshot
{
    public AccessRegistrySnapshot(
        IReadOnlyDictionary<string, EndpointAccessAuthorizationDto> endpoints,
        IReadOnlyDictionary<long, IReadOnlyCollection<string>> featureKeysByAccess)
    {
        Endpoints = endpoints;
        FeatureKeysByAccess = featureKeysByAccess;
    }

    public IReadOnlyDictionary<string, EndpointAccessAuthorizationDto> Endpoints { get; }

    public IReadOnlyDictionary<long, IReadOnlyCollection<string>> FeatureKeysByAccess { get; }

    public static string IdentityOf(string application, string httpMethod, string routePattern) =>
        $"{application}\n{EndpointAccessIdentity.NormalizeMethod(httpMethod)}\n{EndpointAccessIdentity.NormalizeRoute(routePattern)}";
}

/// <summary>
/// Process-wide holder of the current <see cref="AccessRegistrySnapshot"/> (built-in memory only;
/// one instance per process). Invalidated after catalog sync and after any Access/Feature change.
/// </summary>
public interface IAccessRegistryCache
{
    /// <summary>Increases on every invalidation; a load started before it changed is discarded.</summary>
    long Generation { get; }

    AccessRegistrySnapshot? Current { get; }

    /// <summary>Stores a snapshot loaded at <paramref name="loadedAtGeneration"/>, unless invalidated since.</summary>
    void Set(AccessRegistrySnapshot snapshot, long loadedAtGeneration);

    void Invalidate();
}
