using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using PM.Identities.Core.Contract.Accesses;
using PM.Identities.Core.Contract.Accesses.DTOs;
using PM.Identities.Core.Domain.Accesses;
using PM.Identities.Core.Domain.Features;
using PM.Identities.Infra.DAL.DbContext;

namespace PM.Identities.Core.ApplicationService.Accesses;

public sealed class AccessCatalogSyncService : IAccessCatalogSyncService
{
    private static readonly HashSet<string> SupportedMethods =
        ["GET", "POST", "PUT", "PATCH", "DELETE", "HEAD", "OPTIONS"];

    private readonly PMIdentityDbContext _context;
    private readonly IAdminAccessCacheInvalidator _cacheInvalidator;

    public AccessCatalogSyncService(
        PMIdentityDbContext context,
        IAdminAccessCacheInvalidator cacheInvalidator)
    {
        _context = context;
        _cacheInvalidator = cacheInvalidator;
    }

    public async Task<AccessCatalogSyncResult> SyncAsync(
        IReadOnlyCollection<EndpointAccessDefinitionDto> definitions,
        CancellationToken cancellationToken = default)
    {
        var normalized = definitions.Select(NormalizeAndValidate).ToList();
        EnsureUnique(normalized);

        IDbContextTransaction? transaction = null;
        if (_context.Database.IsRelational())
        {
            transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        }

        try
        {
            var features = await _context.Features.ToDictionaryAsync(
                feature => AccessKeyNormalizer.Normalize(feature.Key),
                StringComparer.Ordinal,
                cancellationToken);
            var accesses = await _context.Accesses
                .Include(access => access.FeatureAccesses)
                .ToListAsync(cancellationToken);
            var byKey = accesses.ToDictionary(
                access => EndpointAccessIdentity.NormalizeKey(access.Key),
                StringComparer.Ordinal);
            var byIdentity = accesses
                .Where(access => !string.IsNullOrWhiteSpace(access.HttpMethod)
                    && !string.IsNullOrWhiteSpace(access.RoutePattern))
                .ToDictionary(IdentityOf, StringComparer.Ordinal);

            var catalogIdentities = normalized.Select(IdentityOf).ToHashSet(StringComparer.Ordinal);
            var catalogKeys = normalized.Select(item => item.Key).ToHashSet(StringComparer.Ordinal);
            var staleManagedKeys = accesses
                .Where(access => access.Application == EndpointAccessIdentity.Application
                    && !catalogIdentities.Contains(IdentityOf(access))
                    && !catalogKeys.Contains(EndpointAccessIdentity.NormalizeKey(access.Key)))
                .Select(access => access.Key)
                .OrderBy(key => key, StringComparer.Ordinal)
                .ToArray();
            if (staleManagedKeys.Length > 0)
            {
                throw new InvalidOperationException(
                    $"Persisted endpoint Access definitions no longer exist at runtime: {string.Join(", ", staleManagedKeys)}. " +
                    "Run the controlled TASK-0168 cleanup after reviewing the target environment.");
            }

            var inserted = 0;
            var updated = 0;
            var linksAdded = 0;
            const int linksRemoved = 0;
            var now = DateTime.UtcNow;

            foreach (var definition in normalized)
            {
                var identity = IdentityOf(definition);
                var isNew = false;
                if (!byKey.TryGetValue(definition.Key, out var access)
                    && !byIdentity.TryGetValue(identity, out access))
                {
                    access = new Access
                    {
                        CreatedAt = now,
                        FeatureAccesses = []
                    };
                    _context.Accesses.Add(access);
                    accesses.Add(access);
                    inserted++;
                    isNew = true;
                }

                if (Apply(access, definition, now))
                {
                    updated += access.Id == 0 ? 0 : 1;
                }

                byKey[definition.Key] = access;
                byIdentity[identity] = access;

                var desiredFeatureIds = new HashSet<long>();
                foreach (var featureKey in definition.FeatureKeys)
                {
                    var normalizedFeatureKey = AccessKeyNormalizer.Normalize(featureKey);
                    if (!features.TryGetValue(normalizedFeatureKey, out var feature))
                    {
                        throw new InvalidOperationException(
                            $"Feature '{featureKey}' required by endpoint '{definition.Key}' does not exist.");
                    }

                    desiredFeatureIds.Add(feature.Id);
                }

                if (!isNew)
                {
                    continue;
                }

                foreach (var featureId in desiredFeatureIds)
                {
                    access.FeatureAccesses.Add(new FeatureAccess
                    {
                        FeatureId = featureId,
                        CreatedAt = now
                    });
                    linksAdded++;
                }
            }

            await _context.SaveChangesAsync(cancellationToken);
            if (transaction is not null)
            {
                await transaction.CommitAsync(cancellationToken);
            }

            if (inserted + updated + linksAdded + linksRemoved > 0)
            {
                _cacheInvalidator.InvalidateAll();
            }

            return new AccessCatalogSyncResult(normalized.Count, inserted, updated, linksAdded, linksRemoved);
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
    }

    private static EndpointAccessDefinitionDto NormalizeAndValidate(EndpointAccessDefinitionDto definition)
    {
        var normalized = definition with
        {
            Application = definition.Application.Trim(),
            Module = definition.Module.Trim(),
            Controller = definition.Controller.Trim(),
            Action = definition.Action.Trim(),
            HttpMethod = EndpointAccessIdentity.NormalizeMethod(definition.HttpMethod),
            RoutePattern = EndpointAccessIdentity.NormalizeRoute(definition.RoutePattern),
            Key = EndpointAccessIdentity.NormalizeKey(definition.Key),
            PersianTitle = definition.PersianTitle.Trim(),
            FeatureKeys = definition.FeatureKeys
                .Where(key => !string.IsNullOrWhiteSpace(key))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray()
        };

        if (string.IsNullOrWhiteSpace(normalized.Application)
            || string.IsNullOrWhiteSpace(normalized.Key)
            || string.IsNullOrWhiteSpace(normalized.PersianTitle)
            || string.IsNullOrWhiteSpace(normalized.RoutePattern)
            || !SupportedMethods.Contains(normalized.HttpMethod)
            || normalized.AccessMode is < 1 or > 4
            || !Enum.IsDefined((AccessPanel)normalized.Panel)
            || (AccessPanel)normalized.Panel == AccessPanel.None
            || normalized.Requirements > (byte)(AccessRequirements.ApprovedMember | AccessRequirements.ActiveBuyer))
        {
            throw new InvalidOperationException($"Invalid endpoint Access definition '{definition.Key}'.");
        }

        return normalized;
    }

    private static void EnsureUnique(IReadOnlyCollection<EndpointAccessDefinitionDto> definitions)
    {
        var duplicateKey = definitions.GroupBy(item => item.Key, StringComparer.Ordinal)
            .FirstOrDefault(group => group.Count() > 1);
        if (duplicateKey is not null)
        {
            throw new InvalidOperationException($"Duplicate Access key '{duplicateKey.Key}'.");
        }

        var duplicateIdentity = definitions.GroupBy(IdentityOf, StringComparer.Ordinal)
            .FirstOrDefault(group => group.Count() > 1);
        if (duplicateIdentity is not null)
        {
            throw new InvalidOperationException($"Duplicate endpoint identity '{duplicateIdentity.Key}'.");
        }
    }

    private static bool Apply(Access access, EndpointAccessDefinitionDto definition, DateTime now)
    {
        var changed = access.Key != definition.Key
            || access.Application != definition.Application
            || access.PersianTitle != definition.PersianTitle
            || access.HttpMethod != definition.HttpMethod
            || access.RoutePattern != definition.RoutePattern
            || (byte)access.AccessMode != definition.AccessMode
            || (byte)access.Panel != definition.Panel
            || (byte)access.Requirements != definition.Requirements
            || !access.Enable;

        if (!changed)
        {
            return false;
        }

        access.Key = definition.Key;
        access.Application = definition.Application;
        access.PersianTitle = definition.PersianTitle;
        access.HttpMethod = definition.HttpMethod;
        access.RoutePattern = definition.RoutePattern;
        access.AccessMode = (AccessMode)definition.AccessMode;
        access.Panel = (AccessPanel)definition.Panel;
        access.Requirements = (AccessRequirements)definition.Requirements;
        access.Enable = true;
        access.UpdatedAt = now;
        return true;
    }

    private static string IdentityOf(Access access) =>
        $"{access.Application}\n{EndpointAccessIdentity.NormalizeMethod(access.HttpMethod)}\n{EndpointAccessIdentity.NormalizeRoute(access.RoutePattern)}";

    private static string IdentityOf(EndpointAccessDefinitionDto definition) =>
        $"{definition.Application}\n{definition.HttpMethod}\n{definition.RoutePattern}";
}

