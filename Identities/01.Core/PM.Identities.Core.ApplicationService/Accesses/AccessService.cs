using Microsoft.EntityFrameworkCore;
using PM.Identities.Core.Contract.Accesses.DTOs;
using Poshteh.Core.Domain;
using PM.Identities.Core.Contract.Accesses;
using PM.Identities.Core.Domain.Accesses;
using PM.Identities.Core.Domain.Errors;
using PM.Identities.Infra.DAL.DbContext;

namespace PM.Identities.Core.ApplicationService.Accesses;

public class AccessService : IAccessService
{
    private const int DefaultPageSize = 20;
    private const int MaxPageSize = 100;

    private readonly PMIdentityDbContext _context;
    private readonly IAdminAccessCacheInvalidator _cacheInvalidator;

    public AccessService(PMIdentityDbContext context, IAdminAccessCacheInvalidator cacheInvalidator)
    {
        _context = context;
        _cacheInvalidator = cacheInvalidator;
    }

    public async Task<Result<AccessDto>> CreateAccessAsync(CreateAccessDto dto, CancellationToken cancellationToken = default)
    {
        var key = EndpointAccessIdentity.NormalizeKey(dto.Key);
        if (await _context.Accesses.AnyAsync(access => access.Key == key, cancellationToken))
        {
            return Result.Failure<AccessDto>(IdentityErrors.AccessAlreadyExists(key));
        }

        var access = new Access
        {
            Key = key,
            Application = dto.Application.Trim(),
            PersianTitle = dto.PersianTitle.Trim(),
            HttpMethod = EndpointAccessIdentity.NormalizeMethod(dto.HttpMethod),
            RoutePattern = EndpointAccessIdentity.NormalizeRoute(dto.RoutePattern),
            AccessMode = (AccessMode)dto.AccessMode,
            Enable = dto.Enable,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Accesses.Add(access);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success(Map(access));
    }

    public async Task<Result<AccessDto>> UpdateAccessAsync(long id, UpdateAccessDto dto, CancellationToken cancellationToken = default)
    {
        var access = await _context.Accesses.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (access is null)
        {
            return Result.Failure<AccessDto>(IdentityErrors.AccessNotFound(id));
        }

        var key = EndpointAccessIdentity.NormalizeKey(dto.Key);
        if (await _context.Accesses.AnyAsync(item => item.Id != id && item.Key == key, cancellationToken))
        {
            return Result.Failure<AccessDto>(IdentityErrors.AccessAlreadyExists(key));
        }

        access.Key = key;
        access.Application = dto.Application.Trim();
        access.PersianTitle = dto.PersianTitle.Trim();
        access.HttpMethod = EndpointAccessIdentity.NormalizeMethod(dto.HttpMethod);
        access.RoutePattern = EndpointAccessIdentity.NormalizeRoute(dto.RoutePattern);
        access.AccessMode = (AccessMode)dto.AccessMode;
        access.Enable = dto.Enable;
        access.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        _cacheInvalidator.InvalidateAll();
        return Result.Success(Map(access));
    }

    public async Task<Result> DeleteAccessAsync(long id, CancellationToken cancellationToken = default)
    {
        var access = await _context.Accesses.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (access is null)
        {
            return Result.Failure(IdentityErrors.AccessNotFound(id));
        }

        _context.Accesses.Remove(access);
        await _context.SaveChangesAsync(cancellationToken);
        _cacheInvalidator.InvalidateAll();
        return Result.Success();
    }

    public async Task<Result<AccessDto>> GetAccessByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var access = await _context.Accesses.AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        return access is null
            ? Result.Failure<AccessDto>(IdentityErrors.AccessNotFound(id))
            : Result.Success(Map(access));
    }

    public async Task<Result<AccessPageDto>> GetAccessListAsync(
        AccessListQuery query,
        CancellationToken cancellationToken = default)
    {
        var pageNumber = query.PageNumber < 1 ? 1 : query.PageNumber;
        var pageSize = query.PageSize < 1 ? DefaultPageSize : Math.Min(query.PageSize, MaxPageSize);
        var term = query.Search?.Trim().ToLowerInvariant();

        var accesses = _context.Accesses.AsNoTracking();
        if (!string.IsNullOrEmpty(term))
        {
            accesses = accesses.Where(access =>
                access.PersianTitle.ToLower().Contains(term)
                || access.Key.ToLower().Contains(term)
                || access.RoutePattern.ToLower().Contains(term));
        }

        var totalCount = await accesses.CountAsync(cancellationToken);
        var items = await accesses
            .OrderBy(access => access.PersianTitle)
            .ThenBy(access => access.Key)
            .ThenBy(access => access.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return Result.Success(new AccessPageDto
        {
            Items = items.Select(Map).ToList(),
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        });
    }

    public async Task<Result<List<AccessResourceNodeDto>>> GetResourceTreeAsync(
        IReadOnlyCollection<EndpointAccessDefinitionDto> definitions,
        CancellationToken cancellationToken = default)
    {
        var accesses = await _context.Accesses.AsNoTracking().ToListAsync(cancellationToken);
        var byKey = accesses.ToDictionary(
            access => EndpointAccessIdentity.NormalizeKey(access.Key),
            StringComparer.Ordinal);
        var byIdentity = accesses
            .Where(access => !string.IsNullOrWhiteSpace(access.HttpMethod)
                && !string.IsNullOrWhiteSpace(access.RoutePattern))
            .GroupBy(IdentityOf, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.Ordinal);

        var leaves = definitions.Select(definition =>
        {
            var key = EndpointAccessIdentity.NormalizeKey(definition.Key);
            var method = EndpointAccessIdentity.NormalizeMethod(definition.HttpMethod);
            var route = EndpointAccessIdentity.NormalizeRoute(definition.RoutePattern);
            var identity = $"{definition.Application.Trim()}\n{method}\n{route}";
            if (!byKey.TryGetValue(key, out var access))
            {
                byIdentity.TryGetValue(identity, out access);
            }

            return new
            {
                definition.Module,
                definition.ModuleTitle,
                definition.Controller,
                definition.ControllerTitle,
                definition.Action,
                Access = access,
                Key = access?.Key ?? key,
                PersianTitle = access?.PersianTitle ?? definition.PersianTitle,
                HttpMethod = access?.HttpMethod ?? method,
                RoutePattern = access?.RoutePattern ?? route,
                Enable = access?.Enable ?? true
            };
        }).ToList();

        var modules = leaves
            .GroupBy(leaf => leaf.Module, StringComparer.Ordinal)
            .OrderBy(group => group.Key, StringComparer.OrdinalIgnoreCase)
            .Select(module => new AccessResourceNodeDto
            {
                Id = $"module:{module.Key}",
                Title = module.First().ModuleTitle,
                NodeType = "module",
                Module = module.Key,
                Children = module
                    .GroupBy(leaf => leaf.Controller, StringComparer.Ordinal)
                    .OrderBy(group => group.Key, StringComparer.OrdinalIgnoreCase)
                    .Select(controller => new AccessResourceNodeDto
                    {
                        Id = $"controller:{module.Key}:{controller.Key}",
                        Title = controller.First().ControllerTitle,
                        NodeType = "controller",
                        Module = module.Key,
                        Controller = controller.Key,
                        Children = controller
                            .OrderBy(leaf => leaf.Action, StringComparer.OrdinalIgnoreCase)
                            .ThenBy(leaf => leaf.HttpMethod, StringComparer.OrdinalIgnoreCase)
                            .Select(leaf => new AccessResourceNodeDto
                            {
                                Id = $"access:{module.Key}:{controller.Key}:{leaf.Action}:{leaf.HttpMethod}",
                                Title = leaf.PersianTitle,
                                NodeType = "access",
                                AccessId = leaf.Access?.Id,
                                Key = leaf.Key,
                                HttpMethod = leaf.HttpMethod,
                                RoutePattern = leaf.RoutePattern,
                                Action = leaf.Action,
                                Module = module.Key,
                                Controller = controller.Key,
                                Enable = leaf.Enable
                            })
                            .ToList()
                    })
                    .ToList()
            })
            .ToList();

        return Result.Success(modules);
    }

    private static string IdentityOf(Access access) =>
        $"{access.Application.Trim()}\n{EndpointAccessIdentity.NormalizeMethod(access.HttpMethod)}\n{EndpointAccessIdentity.NormalizeRoute(access.RoutePattern)}";

    private static AccessDto Map(Access access) => new()
    {
        Id = access.Id,
        Key = access.Key,
        Application = access.Application,
        PersianTitle = access.PersianTitle,
        HttpMethod = access.HttpMethod,
        RoutePattern = access.RoutePattern,
        AccessMode = (byte)access.AccessMode,
        Enable = access.Enable
    };
}
