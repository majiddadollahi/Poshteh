
namespace PM.Identities.Core.ApplicationService.Accesses;

internal sealed class AdminAccessCacheEntry
{
    public long Version;
    public string[]? Keys;
    /// <summary>TASK-0247: full-admin role membership read from the database; null = not loaded.</summary>
    public bool? IsAdmin;
}
