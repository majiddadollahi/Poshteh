namespace PM.Identities.Core.Contract.Accesses;

/// <summary>
/// Normalizes Access keys for case-insensitive comparison and Set membership.
/// </summary>
public static class AccessKeyNormalizer
{
    public static string Normalize(string? key) =>
        string.IsNullOrWhiteSpace(key) ? string.Empty : key.Trim().ToUpperInvariant();

    public static bool Equals(string? left, string? right) =>
        string.Equals(Normalize(left), Normalize(right), StringComparison.Ordinal);

    public static bool StartsWith(string? key, string? prefix) =>
        Normalize(key).StartsWith(Normalize(prefix), StringComparison.Ordinal);

    public static HashSet<string> ToNormalizedSet(IEnumerable<string> keys) =>
        keys
            .Select(Normalize)
            .Where(k => k.Length > 0)
            .ToHashSet(StringComparer.Ordinal);
}
