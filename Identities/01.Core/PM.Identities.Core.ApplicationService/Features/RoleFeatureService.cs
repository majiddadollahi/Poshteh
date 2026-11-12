using Microsoft.EntityFrameworkCore;
using PM.Identities.Core.Contract.Features.DTOs;
using Poshteh.Core.Domain;
using PM.Identities.Core.Contract.Accesses;
using PM.Identities.Core.Contract.Features;
using PM.Identities.Core.Domain.Errors;
using PM.Identities.Core.Domain.Roles;
using PM.Identities.Infra.DAL.DbContext;

namespace PM.Identities.Core.ApplicationService.Features;

public class RoleFeatureService : IRoleFeatureService
{
    private readonly PMIdentityDbContext _context;
    private readonly IAdminAccessCacheInvalidator _cacheInvalidator;

    public RoleFeatureService(PMIdentityDbContext context, IAdminAccessCacheInvalidator cacheInvalidator)
    {
        _context = context;
        _cacheInvalidator = cacheInvalidator;
    }

    public async Task<Result<List<FeatureDto>>> GetRoleFeaturesAsync(long roleId, CancellationToken cancellationToken = default)
    {
        if (!await RoleExistsAsync(roleId, cancellationToken))
        {
            return Result.Failure<List<FeatureDto>>(IdentityErrors.RoleNotFound(roleId));
        }

        var features = await _context.RoleFeatures.AsNoTracking()
            .Where(link => link.RoleId == roleId && link.Feature.Enable)
            .Select(link => new FeatureDto
            {
                Id = link.Feature.Id,
                Key = link.Feature.Key,
                NumericCode = link.Feature.NumericCode,
                PersianTitle = link.Feature.PersianTitle,
                Enable = link.Feature.Enable,
                ParentId = link.Feature.ParentId,
                Dependent = link.Feature.Dependent
            })
            .ToListAsync(cancellationToken);
        return Result.Success(features);
    }

    public async Task<Result<List<RoleFeatureSelectionDto>>> GetAllFeaturesWithSelectionAsync(long roleId, CancellationToken cancellationToken = default)
    {
        if (!await RoleExistsAsync(roleId, cancellationToken))
        {
            return Result.Failure<List<RoleFeatureSelectionDto>>(IdentityErrors.RoleNotFound(roleId));
        }

        var selectedIds = await _context.RoleFeatures.AsNoTracking()
            .Where(link => link.RoleId == roleId)
            .Select(link => link.FeatureId)
            .ToListAsync(cancellationToken);
        var selected = selectedIds.ToHashSet();

        var features = await _context.Features.AsNoTracking()
            .Where(feature => feature.Enable)
            .Select(feature => new RoleFeatureSelectionDto
            {
                Id = feature.Id,
                Key = feature.Key,
                NumericCode = feature.NumericCode,
                PersianTitle = feature.PersianTitle,
                Enable = feature.Enable,
                ParentId = feature.ParentId,
                Dependent = feature.Dependent,
                Selected = selected.Contains(feature.Id)
            })
            .ToListAsync(cancellationToken);
        return Result.Success(features);
    }

    public Task<Result> AddFeatureToRoleAsync(long roleId, long featureId, CancellationToken cancellationToken = default) =>
        AddFeaturesAsync(roleId, [featureId], cancellationToken);

    public Task<Result> RemoveFeatureFromRoleAsync(long roleId, long featureId, CancellationToken cancellationToken = default) =>
        RemoveFeaturesAsync(roleId, [featureId], cancellationToken);

    public async Task<Result> AddFeatureTreeToRoleAsync(long roleId, long featureId, CancellationToken cancellationToken = default)
    {
        var ids = await GetTreeIdsAsync(featureId, cancellationToken);
        return ids.IsFailure ? Result.Failure(ids.Error) : await AddFeaturesAsync(roleId, ids.Value, cancellationToken);
    }

    public async Task<Result> RemoveFeatureTreeFromRoleAsync(long roleId, long featureId, CancellationToken cancellationToken = default)
    {
        var ids = await GetTreeIdsAsync(featureId, cancellationToken);
        return ids.IsFailure ? Result.Failure(ids.Error) : await RemoveFeaturesAsync(roleId, ids.Value, cancellationToken);
    }

    private async Task<Result<List<long>>> GetTreeIdsAsync(long featureId, CancellationToken cancellationToken)
    {
        var nodes = await _context.Features.AsNoTracking()
            .Select(feature => new { feature.Id, feature.ParentId })
            .ToListAsync(cancellationToken);
        if (nodes.All(feature => feature.Id != featureId))
        {
            return Result.Failure<List<long>>(IdentityErrors.FeatureNotFound(featureId));
        }

        var children = nodes.Where(node => node.ParentId.HasValue)
            .GroupBy(node => node.ParentId!.Value)
            .ToDictionary(group => group.Key, group => group.Select(node => node.Id).ToList());
        var result = new HashSet<long>();
        var pending = new Stack<long>([featureId]);
        while (pending.TryPop(out var current))
        {
            if (!result.Add(current) || !children.TryGetValue(current, out var childIds))
            {
                continue;
            }

            foreach (var childId in childIds)
            {
                pending.Push(childId);
            }
        }

        return Result.Success(result.ToList());
    }

    private async Task<Result> AddFeaturesAsync(long roleId, IEnumerable<long> featureIds, CancellationToken cancellationToken)
    {
        if (!await RoleExistsAsync(roleId, cancellationToken))
        {
            return Result.Failure(IdentityErrors.RoleNotFound(roleId));
        }

        var ids = featureIds.Distinct().ToList();
        var existingIds = await _context.Features.Where(feature => ids.Contains(feature.Id))
            .Select(feature => feature.Id).ToListAsync(cancellationToken);
        if (existingIds.Count != ids.Count)
        {
            return Result.Failure(IdentityErrors.FeatureNotFound(ids.Except(existingIds).First()));
        }

        var assigned = await _context.RoleFeatures.Where(link => link.RoleId == roleId && ids.Contains(link.FeatureId))
            .Select(link => link.FeatureId).ToListAsync(cancellationToken);
        var additions = ids.Except(assigned).Select(featureId => new RoleFeature { RoleId = roleId, FeatureId = featureId });
        _context.RoleFeatures.AddRange(additions);
        await _context.SaveChangesAsync(cancellationToken);
        await _cacheInvalidator.InvalidateByRoleAsync(roleId, cancellationToken);
        return Result.Success();
    }

    private async Task<Result> RemoveFeaturesAsync(long roleId, IEnumerable<long> featureIds, CancellationToken cancellationToken)
    {
        if (!await RoleExistsAsync(roleId, cancellationToken))
        {
            return Result.Failure(IdentityErrors.RoleNotFound(roleId));
        }

        var ids = featureIds.Distinct().ToList();
        var links = await _context.RoleFeatures
            .Where(link => link.RoleId == roleId && ids.Contains(link.FeatureId))
            .ToListAsync(cancellationToken);
        _context.RoleFeatures.RemoveRange(links);
        await _context.SaveChangesAsync(cancellationToken);
        await _cacheInvalidator.InvalidateByRoleAsync(roleId, cancellationToken);
        return Result.Success();
    }

    private Task<bool> RoleExistsAsync(long roleId, CancellationToken cancellationToken) =>
        _context.Roles.AnyAsync(role => role.Id == roleId, cancellationToken);

}
