using PM.Identities.Core.Contract.Accesses.DTOs;
using Poshteh.Core.Domain;
using PM.Identities.Core.Contract.Accesses;
using PM.Identities.Core.Contract.Auth;
using PM.Identities.Core.Domain.Errors;
using PM.Identities.Infra.DAL.DbContext;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace PM.Identities.Core.ApplicationService.Accesses;

public class AdminAccessQueryService : IAdminAccessQueryService
{
    private readonly PMIdentityDbContext _context;
    private readonly IAdminAccessEvaluator _evaluator;
    private readonly IAdminAccessCache _cache;

    public AdminAccessQueryService(
        PMIdentityDbContext context,
        IAdminAccessEvaluator evaluator,
        IAdminAccessCache cache)
    {
        _context = context;
        _evaluator = evaluator;
        _cache = cache;
    }

    public async Task<Result<MyAdminAccessesDto>> GetMyAccessesAsync(
        long userId,
        CancellationToken cancellationToken = default)
    {
        var exists = await _context.Users.AsNoTracking()
            .AnyAsync(u => u.Id == userId, cancellationToken);
        if (!exists)
        {
            return Result.Failure<MyAdminAccessesDto>(IdentityErrors.UserNotFound(userId));
        }

        var principal = await CreatePrincipalAsync(userId, cancellationToken);
        var isAdmin = await _evaluator.IsAdminAsync(principal, cancellationToken);
        var keys = await _evaluator.GetAccessKeysAsync(principal, cancellationToken);
        var canEnter = isAdmin || await _evaluator.HasAnyAdminPanelAccessAsync(principal, cancellationToken);
        var orgCodes = isAdmin
            ? []
            : (await _evaluator.GetManagedOrganizationCodesAsync(principal, cancellationToken)).ToList();

        return Result.Success(new MyAdminAccessesDto
        {
            IsAdmin = isAdmin,
            CanEnterAdminPanel = canEnter,
            Keys = keys.ToList(),
            ManagedOrganizationCodes = orgCodes,
            AccessVersion = _cache.GetVersion(userId)
        });
    }

    public async Task<Result<MyAdminAccessVersionDto>> GetMyAccessVersionAsync(
        long userId,
        CancellationToken cancellationToken = default)
    {
        var exists = await _context.Users.AsNoTracking()
            .AnyAsync(u => u.Id == userId, cancellationToken);
        if (!exists)
        {
            return Result.Failure<MyAdminAccessVersionDto>(IdentityErrors.UserNotFound(userId));
        }

        return Result.Success(new MyAdminAccessVersionDto
        {
            AccessVersion = _cache.GetVersion(userId)
        });
    }

    private async Task<ClaimsPrincipal> CreatePrincipalAsync(long userId, CancellationToken cancellationToken)
    {
        var roleNames = await (
            from ur in _context.UserRoles.AsNoTracking()
            join r in _context.Roles.AsNoTracking() on ur.RoleId equals r.Id
            where ur.UserId == userId
            select r.Name).ToListAsync(cancellationToken);

        var claims = new List<Claim>
        {
            new(MahtaJwtClaimTypes.UserId, userId.ToString()),
            new(ClaimTypes.NameIdentifier, userId.ToString())
        };

        foreach (var role in roleNames.Where(n => !string.IsNullOrWhiteSpace(n)))
        {
            claims.Add(new Claim(ClaimTypes.Role, role!));
        }

        return new ClaimsPrincipal(new ClaimsIdentity(claims, authenticationType: "AdminAccessQuery"));
    }
}
