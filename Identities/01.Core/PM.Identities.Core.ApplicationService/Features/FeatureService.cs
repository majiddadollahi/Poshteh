using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using PM.Identities.Core.Contract.Features.DTOs;
using Poshteh.Core.Domain;
using PM.Identities.Core.Contract.Accesses;
using PM.Identities.Core.Contract.Features;
using PM.Identities.Core.Domain.Errors;
using PM.Identities.Core.Domain.Features;
using PM.Identities.Infra.DAL.DbContext;

namespace PM.Identities.Core.ApplicationService.Features;

public class FeatureService : IFeatureService
{
    private readonly PMIdentityDbContext _context;
    private readonly IAdminAccessCacheInvalidator _cacheInvalidator;

    public FeatureService(PMIdentityDbContext context, IAdminAccessCacheInvalidator cacheInvalidator)
    {
        _context = context;
        _cacheInvalidator = cacheInvalidator;
    }

    public async Task<Result<FeatureDto>> CreateFeatureAsync(CreateFeatureDto dto, CancellationToken cancellationToken = default)
    {
        var key = dto.Key.Trim();
        if (await _context.Features.AnyAsync(feature => feature.Key == key, cancellationToken))
        {
            return Result.Failure<FeatureDto>(IdentityErrors.FeatureAlreadyExists(key));
        }

        if (dto.ParentId.HasValue && !await _context.Features.AnyAsync(feature => feature.Id == dto.ParentId, cancellationToken))
        {
            return Result.Failure<FeatureDto>(IdentityErrors.FeatureNotFound(dto.ParentId.Value));
        }

        var feature = new Feature
        {
            Key = key,
            NumericCode = dto.NumericCode,
            PersianTitle = dto.PersianTitle,
            Enable = dto.Enable,
            ParentId = dto.ParentId,
            Dependent = dto.Dependent
        };
        _context.Features.Add(feature);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success(Map(feature));
    }

    public async Task<Result<FeatureDto>> UpdateFeatureAsync(long id, UpdateFeatureDto dto, CancellationToken cancellationToken = default)
    {
        var feature = await _context.Features.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (feature is null)
        {
            return Result.Failure<FeatureDto>(IdentityErrors.FeatureNotFound(id));
        }

        var key = dto.Key.Trim();
        if (await _context.Features.AnyAsync(item => item.Id != id && item.Key == key, cancellationToken))
        {
            return Result.Failure<FeatureDto>(IdentityErrors.FeatureAlreadyExists(key));
        }

        if (dto.ParentId.HasValue && !await _context.Features.AnyAsync(item => item.Id == dto.ParentId, cancellationToken))
        {
            return Result.Failure<FeatureDto>(IdentityErrors.FeatureNotFound(dto.ParentId.Value));
        }

        if (dto.ParentId.HasValue && await ParentCreatesCycleAsync(id, dto.ParentId.Value, cancellationToken))
        {
            return Result.Failure<FeatureDto>(IdentityErrors.FeatureParentInvalid);
        }

        feature.Key = key;
        feature.NumericCode = dto.NumericCode;
        feature.PersianTitle = dto.PersianTitle;
        feature.Enable = dto.Enable;
        feature.ParentId = dto.ParentId;
        feature.Dependent = dto.Dependent;
        await _context.SaveChangesAsync(cancellationToken);
        _cacheInvalidator.InvalidateAll();
        return Result.Success(Map(feature));
    }

    public async Task<Result> DeleteFeatureAsync(long id, CancellationToken cancellationToken = default)
    {
        var feature = await _context.Features.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (feature is null)
        {
            return Result.Failure(IdentityErrors.FeatureNotFound(id));
        }

        _context.Features.Remove(feature);
        await _context.SaveChangesAsync(cancellationToken);
        _cacheInvalidator.InvalidateAll();
        return Result.Success();
    }

    public async Task<Result<FeatureDto>> GetFeatureByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var feature = await _context.Features.AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        return feature is null
            ? Result.Failure<FeatureDto>(IdentityErrors.FeatureNotFound(id))
            : Result.Success(Map(feature));
    }

    public async Task<Result<List<FeatureDto>>> GetFeatureListAsync(CancellationToken cancellationToken = default)
    {
        var features = await _context.Features.AsNoTracking().OrderBy(item => item.Key).ToListAsync(cancellationToken);
        return Result.Success(features.Select(Map).ToList());
    }

    public async Task<Result<List<FeatureTreeNodeDto>>> GetFeatureTreeAsync(CancellationToken cancellationToken = default)
    {
        var features = await _context.Features.AsNoTracking()
            .Select(feature => new FeatureTreeNodeDto
            {
                Id = feature.Id,
                Key = feature.Key,
                NumericCode = feature.NumericCode,
                PersianTitle = feature.PersianTitle,
                Enable = feature.Enable,
                ParentId = feature.ParentId,
                Dependent = feature.Dependent,
                AccessCount = feature.FeatureAccesses.Count
            })
            .ToListAsync(cancellationToken);

        return Result.Success(BuildFeatureTree(features));
    }

    public async Task<Result<List<FeatureLinkedAccessDto>>> GetFeatureAccessesAsync(
        long featureId, CancellationToken cancellationToken = default)
    {
        if (!await _context.Features.AnyAsync(feature => feature.Id == featureId, cancellationToken))
        {
            return Result.Failure<List<FeatureLinkedAccessDto>>(IdentityErrors.FeatureNotFound(featureId));
        }

        var accesses = await _context.FeatureAccesses.AsNoTracking()
            .Where(link => link.FeatureId == featureId)
            .Select(link => new FeatureLinkedAccessDto
            {
                Id = link.Access.Id,
                Key = link.Access.Key,
                PersianTitle = link.Access.PersianTitle,
                HttpMethod = link.Access.HttpMethod,
                RoutePattern = link.Access.RoutePattern,
                Application = link.Access.Application,
                Enable = link.Access.Enable
            })
            .ToListAsync(cancellationToken);

        return Result.Success(accesses
            .OrderBy(access => access.PersianTitle, StringComparer.OrdinalIgnoreCase)
            .ThenBy(access => access.Key, StringComparer.OrdinalIgnoreCase)
            .ToList());
    }

    public async Task<Result> ReplaceFeatureAccessesAsync(
        long featureId,
        ReplaceFeatureAccessesDto dto,
        CancellationToken cancellationToken = default)
    {
        if (!await _context.Features.AnyAsync(feature => feature.Id == featureId, cancellationToken))
        {
            return Result.Failure(IdentityErrors.FeatureNotFound(featureId));
        }

        var requestedIds = dto.AccessIds ?? [];
        var invalidId = requestedIds.FirstOrDefault(id => id <= 0);
        if (requestedIds.Any(id => id <= 0))
        {
            return Result.Failure(IdentityErrors.AccessNotFound(invalidId));
        }

        var desiredIds = requestedIds.Distinct().ToArray();
        if (desiredIds.Length > 0)
        {
            var existingIds = await _context.Accesses.AsNoTracking()
                .Where(access => desiredIds.Contains(access.Id))
                .Select(access => access.Id)
                .ToListAsync(cancellationToken);
            var missingIds = desiredIds.Except(existingIds).OrderBy(id => id).ToArray();
            if (missingIds.Length > 0)
            {
                return Result.Failure(IdentityErrors.AccessNotFound(missingIds[0]));
            }
        }

        var currentLinks = await _context.FeatureAccesses
            .Where(link => link.FeatureId == featureId)
            .ToListAsync(cancellationToken);
        var currentIds = currentLinks.Select(link => link.AccessId).ToHashSet();
        var desired = desiredIds.ToHashSet();
        var toRemove = currentLinks.Where(link => !desired.Contains(link.AccessId)).ToList();
        var toAdd = desired
            .Where(id => !currentIds.Contains(id))
            .Select(id => new FeatureAccess
            {
                FeatureId = featureId,
                AccessId = id,
                CreatedAt = DateTime.UtcNow
            })
            .ToList();

        if (toRemove.Count == 0 && toAdd.Count == 0)
        {
            return Result.Success();
        }

        IDbContextTransaction? transaction = null;
        if (_context.Database.IsRelational())
        {
            transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        }

        try
        {
            if (toRemove.Count > 0)
            {
                _context.FeatureAccesses.RemoveRange(toRemove);
            }

            if (toAdd.Count > 0)
            {
                _context.FeatureAccesses.AddRange(toAdd);
            }

            await _context.SaveChangesAsync(cancellationToken);
            if (transaction is not null)
            {
                await transaction.CommitAsync(cancellationToken);
            }
        }
        catch
        {
            if (transaction is not null)
            {
                await transaction.RollbackAsync(cancellationToken);
            }

            throw;
        }
        finally
        {
            if (transaction is not null)
            {
                await transaction.DisposeAsync();
            }
        }

        await _cacheInvalidator.InvalidateByFeatureAsync(featureId, cancellationToken);
        return Result.Success();
    }

    private async Task<bool> ParentCreatesCycleAsync(long featureId, long parentId, CancellationToken cancellationToken)
    {
        var parents = await _context.Features.AsNoTracking()
            .Select(feature => new { feature.Id, feature.ParentId })
            .ToDictionaryAsync(feature => feature.Id, feature => feature.ParentId, cancellationToken);

        var current = parentId;
        var seen = new HashSet<long>();
        while (seen.Add(current))
        {
            if (current == featureId)
            {
                return true;
            }

            if (!parents.TryGetValue(current, out var next) || next is null)
            {
                return false;
            }

            current = next.Value;
        }

        return false;
    }

    private static List<FeatureTreeNodeDto> BuildFeatureTree(List<FeatureTreeNodeDto> features)
    {
        var byId = features.ToDictionary(feature => feature.Id);
        var childLists = features.ToDictionary(feature => feature.Id, _ => new List<FeatureTreeNodeDto>());
        var roots = new List<FeatureTreeNodeDto>();

        foreach (var feature in features)
        {
            if (feature.ParentId is long parentId
                && parentId != feature.Id
                && byId.ContainsKey(parentId)
                && !ParentChainContains(feature.Id, parentId, byId))
            {
                childLists[parentId].Add(feature);
            }
            else
            {
                roots.Add(feature);
            }
        }

        foreach (var feature in features)
        {
            feature.Children = childLists[feature.Id]
                .OrderBy(item => item.PersianTitle, StringComparer.OrdinalIgnoreCase)
                .ThenBy(item => item.Key, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        return roots
            .OrderBy(item => item.PersianTitle, StringComparer.OrdinalIgnoreCase)
            .ThenBy(item => item.Key, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static bool ParentChainContains(
        long featureId,
        long parentId,
        IReadOnlyDictionary<long, FeatureTreeNodeDto> byId)
    {
        var current = parentId;
        var seen = new HashSet<long>();
        while (seen.Add(current))
        {
            if (current == featureId)
            {
                return true;
            }

            if (!byId.TryGetValue(current, out var node) || node.ParentId is null)
            {
                return false;
            }

            current = node.ParentId.Value;
        }

        return false;
    }

    private static FeatureDto Map(Feature feature) => new()
    {
        Id = feature.Id,
        Key = feature.Key,
        NumericCode = feature.NumericCode,
        PersianTitle = feature.PersianTitle,
        Enable = feature.Enable,
        ParentId = feature.ParentId,
        Dependent = feature.Dependent
    };
}
