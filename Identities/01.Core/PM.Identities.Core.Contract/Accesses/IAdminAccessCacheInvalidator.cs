namespace PM.Identities.Core.Contract.Accesses;

/// <summary>
/// Invalidates cached admin accesses after role/access membership changes.
/// </summary>
public interface IAdminAccessCacheInvalidator
{
    void InvalidateUser(long userId);

    Task InvalidateByRoleAsync(long roleId, CancellationToken cancellationToken = default);

    Task InvalidateByAccessAsync(long accessId, CancellationToken cancellationToken = default);

    Task InvalidateByFeatureAsync(long featureId, CancellationToken cancellationToken = default);

    void InvalidateAll();
}
