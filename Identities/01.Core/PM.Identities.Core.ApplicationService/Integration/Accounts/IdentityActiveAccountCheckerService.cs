using PM.Identities.Core.Contract.Integration.Accounts.DTOs;
using Poshteh.Core.Domain;
using PM.Identities.Core.Contract.Integration.Accounts;
using PM.Identities.Core.Domain.Errors;
using PM.Identities.Infra.DAL.DbContext;
using Microsoft.EntityFrameworkCore;

namespace PM.Identities.Core.ApplicationService.Integration.Accounts;

public class IdentityActiveAccountCheckerService : IIdentityActiveAccountCheckerContract
{
    private readonly PMIdentityDbContext _context;

    public IdentityActiveAccountCheckerService(PMIdentityDbContext context)
    {
        _context = context;
    }

    public async Task<Result<IdentityActiveAccountCheckResultDto>> CheckActiveAccountAsync(
        IdentityActiveAccountCheckRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var hasIdentifier =
            request.UserId is > 0 ||
            !string.IsNullOrWhiteSpace(request.UserName) ||
            !string.IsNullOrWhiteSpace(request.Email) ||
            !string.IsNullOrWhiteSpace(request.NationalId);

        if (!hasIdentifier)
        {
            return Result.Failure<IdentityActiveAccountCheckResultDto>(
                IdentityErrors.IdentifierRequired);
        }

        var query = _context.Users.AsQueryable();

        if (request.UserId is > 0)
        {
            query = query.Where(x => x.Id == request.UserId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.UserName))
        {
            var userName = request.UserName.Trim();
            query = query.Where(x => x.UserName == userName);
        }

        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            var email = request.Email.Trim();
            query = query.Where(x => x.Email == email);
        }

        if (!string.IsNullOrWhiteSpace(request.NationalId))
        {
            var nationalId = request.NationalId.Trim();
            query = query.Where(x => x.NationalId == nationalId);
        }

        var user = await query.FirstOrDefaultAsync(cancellationToken);

        if (user == null)
        {
            return Result.Success(new IdentityActiveAccountCheckResultDto
            {
                Exists = false,
                IsActive = false
            });
        }

        return Result.Success(new IdentityActiveAccountCheckResultDto
        {
            Exists = true,
            IsActive = user.IsActive,
            UserId = user.Id,
            UserName = user.UserName
        });
    }
}
