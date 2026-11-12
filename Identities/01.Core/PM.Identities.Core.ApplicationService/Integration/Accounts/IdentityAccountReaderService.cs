using PM.Identities.Core.Contract.Integration.Accounts.DTOs;
using Poshteh.Core.Domain;
using PM.Identities.Core.Contract.Integration.Accounts;
using PM.Identities.Core.Domain.Errors;
using PM.Identities.Infra.DAL.DbContext;
using Microsoft.EntityFrameworkCore;

namespace PM.Identities.Core.ApplicationService.Integration.Accounts;

public class IdentityAccountReaderService : IIdentityAccountReaderContract
{
    private readonly PMIdentityDbContext _context;

    public IdentityAccountReaderService(PMIdentityDbContext context)
    {
        _context = context;
    }

    public async Task<Result<IdentityAccountWithRolesAndAccessesDto>> GetAccountWithRolesAndAccessesAsync(
        long userId,
        CancellationToken cancellationToken = default)
    {
        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);

        if (user == null)
        {
            return Result.Failure<IdentityAccountWithRolesAndAccessesDto>(IdentityErrors.UserNotFound(userId));
        }

        var roles = await (
            from ur in _context.UserRoles
            join r in _context.Roles on ur.RoleId equals r.Id
            where ur.UserId == userId
            select new IdentityAccountRoleDto
            {
                RoleId = r.Id,
                Name = r.Name ?? string.Empty
            }).ToListAsync(cancellationToken);

        var accessesRaw = await (
            from ur in _context.UserRoles
            join roleFeature in _context.RoleFeatures on ur.RoleId equals roleFeature.RoleId
            join feature in _context.Features on roleFeature.FeatureId equals feature.Id
            join featureAccess in _context.FeatureAccesses on feature.Id equals featureAccess.FeatureId
            join access in _context.Accesses on featureAccess.AccessId equals access.Id
            where ur.UserId == userId && feature.Enable && access.Enable
            select new IdentityAccountAccessDto
            {
                AccessId = access.Id,
                Key = access.Key,
                PersianTitle = null
            }).ToListAsync(cancellationToken);

        var accesses = accessesRaw
            .GroupBy(x => x.AccessId)
            .Select(x => x.First())
            .OrderBy(x => x.Key)
            .ToList();

        return Result.Success(new IdentityAccountWithRolesAndAccessesDto
        {
            UserId = user.Id,
            UserName = user.UserName ?? string.Empty,
            Email = user.Email ?? string.Empty,
            IsActive = user.IsActive,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Roles = roles,
            Accesses = accesses
        });
    }
}
