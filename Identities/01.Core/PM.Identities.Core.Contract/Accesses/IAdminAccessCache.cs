namespace PM.Identities.Core.Contract.Accesses;

/// <summary>
/// Process-wide cache of resolved Admin Access keys per user, with a monotonic version for UI refresh.
/// </summary>
public interface IAdminAccessCache
{
    bool TryGetKeys(long userId, out IReadOnlyCollection<string> normalizedKeys);

    void SetKeys(long userId, IReadOnlyCollection<string> normalizedKeys);

    /// <summary>TASK-0247: cached database membership of the full-admin role (cleared with the keys).</summary>
    bool TryGetIsAdmin(long userId, out bool isAdmin);

    void SetIsAdmin(long userId, bool isAdmin);

    /// <summary>Clears cached keys and bumps the user's access version.</summary>
    void InvalidateUser(long userId);

    /// <summary>Clears every cached user entry and bumps each known version.</summary>
    void InvalidateAll();

    /// <summary>
    /// Current access version for the user. Increases whenever that user's grants are invalidated
    /// (or when InvalidateAll runs and the user already has an entry / is created by invalidation).
    /// </summary>
    long GetVersion(long userId);
}
