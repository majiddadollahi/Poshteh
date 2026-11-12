using System.Security.Claims;
using PM.Identities.Core.Contract.Accesses;
using PM.Identities.Core.Contract.Accesses.DTOs;
using PM.Identities.Core.Contract.Auth;
using PM.Identities.Core.Domain.Users;
using PM.Identities.Infra.DAL.DbContext;
using Microsoft.EntityFrameworkCore;

namespace PM.Identities.Core.ApplicationService.Accesses;

public class AdminAccessEvaluator : IAdminAccessEvaluator
{
    public const string AdminRoleName = "admin";

    private readonly PMIdentityDbContext _context;
    private readonly IAdminAccessCache _cache;
    private readonly IAccessRegistryCache _registry;

    public AdminAccessEvaluator(
        PMIdentityDbContext context,
        IAdminAccessCache cache,
        IAccessRegistryCache registry)
    {
        _context = context;
        _cache = cache;
        _registry = registry;
    }

    public async Task<bool> HasPanelAccountTypeAsync(ClaimsPrincipal user, CancellationToken cancellationToken = default)
    {
        var userId = ResolveUserId(user);
        if (userId is null)
        {
            return false;
        }

        var accountType = await _context.Users.AsNoTracking()
            .Where(u => u.Id == userId.Value)
            .Select(u => (AccountType?)u.AccountType)
            .SingleOrDefaultAsync(cancellationToken);

        return accountType is not null && accountType.Value.IsPanel();
    }

    /// <summary>
    /// TASK-0247: full-admin membership comes from the user's database roles (cached per user and
    /// cleared by the Role/Access invalidator), never from the token's role claim — revoking the
    /// role takes effect without waiting for the token to expire.
    /// </summary>
    public async Task<bool> IsAdminAsync(ClaimsPrincipal user, CancellationToken cancellationToken = default)
    {
        var userId = ResolveUserId(user);
        if (userId is null)
        {
            return false;
        }

        if (_cache.TryGetIsAdmin(userId.Value, out var cached))
        {
            return cached;
        }

        var normalizedAdmin = AdminRoleName.ToUpperInvariant();
        var isAdmin = await (
            from userRole in _context.UserRoles.AsNoTracking()
            join role in _context.Roles.AsNoTracking() on userRole.RoleId equals role.Id
            where userRole.UserId == userId.Value && role.NormalizedName == normalizedAdmin
            select userRole.UserId)
            .AnyAsync(cancellationToken);

        _cache.SetIsAdmin(userId.Value, isAdmin);
        return isAdmin;
    }

    public async Task<bool> HasAccessAsync(
        ClaimsPrincipal user,
        string key,
        CancellationToken cancellationToken = default)
    {
        if (await IsAdminAsync(user, cancellationToken))
        {
            return true;
        }

        var keys = await GetNormalizedKeysAsync(user, cancellationToken);
        return keys.Contains(AccessKeyNormalizer.Normalize(key));
    }

    public async Task<bool> HasAnyAccessAsync(
        ClaimsPrincipal user,
        IEnumerable<string> keys,
        CancellationToken cancellationToken = default)
    {
        if (await IsAdminAsync(user, cancellationToken))
        {
            return true;
        }

        var owned = await GetNormalizedKeysAsync(user, cancellationToken);
        return keys.Select(AccessKeyNormalizer.Normalize)
            .Where(k => k.Length > 0)
            .Any(owned.Contains);
    }

    public async Task<EndpointAccessAuthorizationDto?> FindEndpointAsync(
        string application,
        string httpMethod,
        string routePattern,
        CancellationToken cancellationToken = default)
    {
        var registry = await GetRegistryAsync(cancellationToken);
        return registry.Endpoints.TryGetValue(
            AccessRegistrySnapshot.IdentityOf(application, httpMethod, routePattern),
            out var endpoint)
            ? endpoint
            : null;
    }

    public async Task<bool> HasAnyFeatureForAccessAsync(
        ClaimsPrincipal user,
        long accessId,
        CancellationToken cancellationToken = default)
    {
        if (await IsAdminAsync(user, cancellationToken))
        {
            return true;
        }

        var owned = await GetNormalizedKeysAsync(user, cancellationToken);
        if (owned.Count == 0)
        {
            return false;
        }

        var registry = await GetRegistryAsync(cancellationToken);
        return registry.FeatureKeysByAccess.TryGetValue(accessId, out var mappedKeys)
            && mappedKeys.Any(owned.Contains);
    }

    /// <summary>
    /// TASK-0247: the endpoint registry (Accesses + enabled FeatureAccess links) is read once and
    /// kept as a process-wide snapshot until the invalidator drops it — two queries per reload,
    /// none per request.
    /// </summary>
    private async Task<AccessRegistrySnapshot> GetRegistryAsync(CancellationToken cancellationToken)
    {
        if (_registry.Current is { } current)
        {
            return current;
        }

        var generation = _registry.Generation;
        var accesses = await _context.Accesses.AsNoTracking()
            .Select(access => new
            {
                access.Id,
                access.Application,
                access.HttpMethod,
                access.RoutePattern,
                access.AccessMode,
                access.Enable,
                access.Panel,
                access.Requirements
            })
            .ToListAsync(cancellationToken);
        var links = await (
            from link in _context.FeatureAccesses.AsNoTracking()
            join feature in _context.Features.AsNoTracking() on link.FeatureId equals feature.Id
            where feature.Enable
            select new { link.AccessId, feature.Key })
            .ToListAsync(cancellationToken);

        // Route identity is compared case-insensitively, as the former SQL lookup did.
        var endpoints = new Dictionary<string, EndpointAccessAuthorizationDto>(StringComparer.OrdinalIgnoreCase);
        foreach (var access in accesses)
        {
            endpoints[AccessRegistrySnapshot.IdentityOf(access.Application, access.HttpMethod, access.RoutePattern)] =
                new EndpointAccessAuthorizationDto(
                    access.Id,
                    (byte)access.AccessMode,
                    access.Enable,
                    (byte)access.Panel,
                    (byte)access.Requirements);
        }

        var featureKeys = links
            .GroupBy(link => link.AccessId)
            .ToDictionary(
                group => group.Key,
                group => (IReadOnlyCollection<string>)group
                    .Select(link => AccessKeyNormalizer.Normalize(link.Key))
                    .ToHashSet(StringComparer.Ordinal));

        var snapshot = new AccessRegistrySnapshot(endpoints, featureKeys);
        _registry.Set(snapshot, generation);
        return snapshot;
    }

    public async Task<bool> HasAnyAdminPanelAccessAsync(
        ClaimsPrincipal user,
        CancellationToken cancellationToken = default)
    {
        if (await IsAdminAsync(user, cancellationToken))
        {
            return true;
        }

        var keys = await GetNormalizedKeysAsync(user, cancellationToken);
        var prefix = AccessKeyNormalizer.Normalize(AdminAccessKeys.Root);
        return keys.Any(k => k == prefix || k.StartsWith(prefix + ".", StringComparison.Ordinal));
    }

    public async Task<bool> CanManageOrganizationAsync(
        ClaimsPrincipal user,
        string tenantCode,
        CancellationToken cancellationToken = default)
    {
        if (await IsAdminAsync(user, cancellationToken))
        {
            return true;
        }

        return await HasAccessAsync(user, AdminAccessOrganizationsKeys.ForTenant(tenantCode), cancellationToken);
    }

    public async Task<bool> HasAnyOrganizationAccessAsync(
        ClaimsPrincipal user,
        CancellationToken cancellationToken = default)
    {
        if (await IsAdminAsync(user, cancellationToken))
        {
            return true;
        }

        var keys = await GetNormalizedKeysAsync(user, cancellationToken);
        var prefix = AccessKeyNormalizer.Normalize(AdminAccessOrganizationsKeys.Node) + ".";
        return keys.Any(k => k.StartsWith(prefix, StringComparison.Ordinal));
    }

    public async Task<IReadOnlyCollection<string>> GetAccessKeysAsync(
        ClaimsPrincipal user,
        CancellationToken cancellationToken = default)
    {
        var keys = await GetNormalizedKeysAsync(user, cancellationToken);
        return keys
            .OrderBy(k => k, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public async Task<IReadOnlyCollection<string>> GetManagedOrganizationCodesAsync(
        ClaimsPrincipal user,
        CancellationToken cancellationToken = default)
    {
        if (await IsAdminAsync(user, cancellationToken))
        {
            return Array.Empty<string>(); // empty means "all" for callers that check IsAdmin first
        }

        var prefix = AccessKeyNormalizer.Normalize(AdminAccessOrganizationsKeys.Node) + ".";
        var catalogCreate = AccessKeyNormalizer.Normalize(AdminAccessOrganizationsKeys.CatalogCreate);
        var keys = await GetNormalizedKeysAsync(user, cancellationToken);

        return keys
            .Where(k => k.StartsWith(prefix, StringComparison.Ordinal) && k != catalogCreate)
            .Select(k => k[prefix.Length..])
            .Where(code => code.Length > 0)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(c => c, StringComparer.Ordinal)
            .ToList();
    }

    private async Task<HashSet<string>> GetNormalizedKeysAsync(
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        var userId = ResolveUserId(user);
        if (userId is null)
        {
            return new HashSet<string>(StringComparer.Ordinal);
        }

        if (_cache.TryGetKeys(userId.Value, out var cached))
        {
            return cached as HashSet<string>
                   ?? new HashSet<string>(cached, StringComparer.Ordinal);
        }

        var raw = await LoadRawKeysAsync(userId.Value, cancellationToken);
        var normalized = AccessKeyNormalizer.ToNormalizedSet(raw);
        _cache.SetKeys(userId.Value, normalized);
        return normalized;
    }

    private async Task<List<string>> LoadRawKeysAsync(long userId, CancellationToken cancellationToken)
    {
        var features = await (
            from ur in _context.UserRoles.AsNoTracking()
            join roleFeature in _context.RoleFeatures.AsNoTracking() on ur.RoleId equals roleFeature.RoleId
            join feature in _context.Features.AsNoTracking() on roleFeature.FeatureId equals feature.Id
            where ur.UserId == userId && feature.Enable
            select new { feature.Id, feature.Key, feature.ParentId, feature.Enable }).Distinct().ToListAsync(cancellationToken);

        if (features.Count == 0)
        {
            return [];
        }

        var allNodes = await _context.Features.AsNoTracking()
            .Select(feature => new { feature.Id, feature.ParentId, feature.Enable })
            .ToDictionaryAsync(feature => feature.Id, cancellationToken);

        bool IsChainEnabled(long featureId)
        {
            var current = allNodes[featureId];
            while (true)
            {
                if (!current.Enable)
                {
                    return false;
                }

                if (current.ParentId is null)
                {
                    return true;
                }

                if (!allNodes.TryGetValue(current.ParentId.Value, out current!))
                {
                    return false;
                }
            }
        }

        return features
            .Where(feature => allNodes.ContainsKey(feature.Id) && IsChainEnabled(feature.Id))
            .Select(feature => feature.Key)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static long? ResolveUserId(ClaimsPrincipal user)
    {
        var raw = user.FindFirstValue(MahtaJwtClaimTypes.UserId)
            ?? user.FindFirstValue(ClaimTypes.NameIdentifier);

        return long.TryParse(raw, out var id) ? id : null;
    }
}
