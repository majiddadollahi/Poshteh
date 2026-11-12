using PM.Identities.Core.Contract.Accesses;
using PM.Identities.Infra.DAL.DbContext;
using Microsoft.EntityFrameworkCore;

namespace PM.Identities.Core.ApplicationService.Accesses;

public sealed class AdminAccessCacheInvalidator : IAdminAccessCacheInvalidator
{
    private readonly IAdminAccessCache _cache;
    private readonly IAccessRegistryCache _registry;
    private readonly PMIdentityDbContext _context;

    public AdminAccessCacheInvalidator(
        IAdminAccessCache cache,
        IAccessRegistryCache registry,
        PMIdentityDbContext context)
    {
        _cache = cache;
        _registry = registry;
        _context = context;
    }

    public void InvalidateUser(long userId) => _cache.InvalidateUser(userId);

    // TASK-0247: every Access/Feature change reaches InvalidateAll, InvalidateByAccessAsync or
    // InvalidateByFeatureAsync, so the endpoint registry snapshot is dropped there (one point).
    public void InvalidateAll()
    {
        _registry.Invalidate();
        _cache.InvalidateAll();
    }

    public async Task InvalidateByRoleAsync(long roleId, CancellationToken cancellationToken = default)
    {
        var userIds = await _context.UserRoles.AsNoTracking()
            .Where(ur => ur.RoleId == roleId)
            .Select(ur => ur.UserId)
            .Distinct()
            .ToListAsync(cancellationToken);

        foreach (var userId in userIds)
        {
            _cache.InvalidateUser(userId);
        }
    }

    public async Task InvalidateByAccessAsync(long accessId, CancellationToken cancellationToken = default)
    {
        _registry.Invalidate();
        var roleIds = await (
            from featureAccess in _context.FeatureAccesses.AsNoTracking()
            join roleFeature in _context.RoleFeatures.AsNoTracking()
                on featureAccess.FeatureId equals roleFeature.FeatureId
            where featureAccess.AccessId == accessId
            select roleFeature.RoleId)
            .Distinct()
            .ToListAsync(cancellationToken);

        foreach (var roleId in roleIds)
        {
            await InvalidateByRoleAsync(roleId, cancellationToken);
        }
    }

    public async Task InvalidateByFeatureAsync(long featureId, CancellationToken cancellationToken = default)
    {
        _registry.Invalidate();
        var roleIds = await _context.RoleFeatures.AsNoTracking()
            .Where(link => link.FeatureId == featureId)
            .Select(link => link.RoleId)
            .Distinct()
            .ToListAsync(cancellationToken);

        foreach (var roleId in roleIds)
        {
            await InvalidateByRoleAsync(roleId, cancellationToken);
        }
    }
}
