using PM.Identities.Core.Contract.Accesses;

namespace PM.Identities.Core.ApplicationService.Accesses;

public sealed class AccessRegistryCache : IAccessRegistryCache
{
    private readonly object _gate = new();
    private AccessRegistrySnapshot? _current;
    private long _generation;

    public long Generation => Interlocked.Read(ref _generation);

    public AccessRegistrySnapshot? Current => Volatile.Read(ref _current);

    public void Set(AccessRegistrySnapshot snapshot, long loadedAtGeneration)
    {
        lock (_gate)
        {
            // A change committed while this snapshot was loading must win over the stale copy.
            if (loadedAtGeneration == _generation)
            {
                Volatile.Write(ref _current, snapshot);
            }
        }
    }

    public void Invalidate()
    {
        lock (_gate)
        {
            _generation++;
            Volatile.Write(ref _current, null);
        }
    }
}
