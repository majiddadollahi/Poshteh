using PM.Identities.Core.Contract.Integration.Accounts.DTOs;
using Poshteh.Core.Domain;

namespace PM.Identities.Core.Contract.Integration.Accounts;

public interface IIdentityAccountReaderContract
{
    Task<Result<IdentityAccountWithRolesAndAccessesDto>> GetAccountWithRolesAndAccessesAsync(
        long userId,
        CancellationToken cancellationToken = default);
}
