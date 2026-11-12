using PM.Identities.Core.Contract.Integration.Accounts.DTOs;
using Poshteh.Core.Domain;

namespace PM.Identities.Core.Contract.Integration.Accounts;

public interface IIdentityAccountCreatorContract
{
    Task<Result<IdentityCreateAccountResultDto>> CreateAccountAsync(
        IdentityCreateAccountRequestDto request,
        CancellationToken cancellationToken = default);
}
