using PM.Identities.Core.Contract.Integration.Users.DTOs;
using Poshteh.Core.Domain;
using PM.Identities.Core.Contract.Integration.Users;
using PM.Identities.Core.Domain.Errors;
using PM.Identities.Infra.DAL.DbContext;
using Microsoft.EntityFrameworkCore;

namespace PM.Identities.Core.ApplicationService.Integration.Users;

public class IdentityUserProfileReaderService : IIdentityUserProfileReaderContract
{
    private readonly PMIdentityDbContext _context;

    public IdentityUserProfileReaderService(PMIdentityDbContext context)
    {
        _context = context;
    }

    public async Task<Result<IdentityUserProfileDto>> GetUserProfileAsync(
        long userId,
        CancellationToken cancellationToken = default)
    {
        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);

        if (user == null)
        {
            return Result.Failure<IdentityUserProfileDto>(IdentityErrors.UserNotFound(userId));
        }

        return Result.Success(IdentityUserProfileMapper.Map(user));
    }
}
