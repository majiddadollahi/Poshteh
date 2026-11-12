using System.Collections.Concurrent;
using PM.Identities.Core.Contract.Accesses;

namespace PM.Identities.Core.ApplicationService.Accesses;

public sealed class AdminAccessCache : IAdminAccessCache
{
    /// <summary>
    /// Changes on process restart so clients with a pre-restart AccessVersion refresh their UI cache.
    /// </summary>
    private static readonly long BootStamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

    private readonly ConcurrentDictionary<long, AdminAccessCacheEntry> _entries = new();
    private long _generation;

    public bool TryGetKeys(long userId, out IReadOnlyCollection<string> normalizedKeys)
    {
        normalizedKeys = Array.Empty<string>();
        if (!_entries.TryGetValue(userId, out var entry) || entry.Keys is null)
        {
            return false;
        }

        normalizedKeys = entry.Keys;
        return true;
    }

    public void SetKeys(long userId, IReadOnlyCollection<string> normalizedKeys)
    {
        var snapshot = normalizedKeys as string[] ?? normalizedKeys.ToArray();
        _entries.AddOrUpdate(
            userId,
            _ => new AdminAccessCacheEntry { Version = 0, Keys = snapshot },
            (_, existing) =>
            {
                existing.Keys = snapshot;
                return existing;
            });
    }

    public bool TryGetIsAdmin(long userId, out bool isAdmin)
    {
        isAdmin = false;
        if (!_entries.TryGetValue(userId, out var entry) || entry.IsAdmin is not { } value)
        {
            return false;
        }

        isAdmin = value;
        return true;
    }

    public void SetIsAdmin(long userId, bool isAdmin)
    {
        _entries.AddOrUpdate(
            userId,
            _ => new AdminAccessCacheEntry { Version = 0, IsAdmin = isAdmin },
            (_, existing) =>
            {
                existing.IsAdmin = isAdmin;
                return existing;
            });
    }

    public void InvalidateUser(long userId)
    {
        _entries.AddOrUpdate(
            userId,
            _ => new AdminAccessCacheEntry { Version = 1, Keys = null },
            (_, existing) =>
            {
                Interlocked.Increment(ref existing.Version);
                existing.Keys = null;
                existing.IsAdmin = null;
                return existing;
            });
    }

    public void InvalidateAll()
    {
        Interlocked.Increment(ref _generation);
        foreach (var userId in _entries.Keys)
        {
            InvalidateUser(userId);
        }
    }

    public long GetVersion(long userId)
    {
        var local = _entries.TryGetValue(userId, out var entry)
            ? Volatile.Read(ref entry.Version)
            : 0;
        var generation = Volatile.Read(ref _generation);
        return BootStamp + generation + local;
    }
}

