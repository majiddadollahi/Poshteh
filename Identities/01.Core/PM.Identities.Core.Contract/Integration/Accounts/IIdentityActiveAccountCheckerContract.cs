using PM.Identities.Core.Contract.Integration.Accounts.DTOs;
using Poshteh.Core.Domain;

namespace PM.Identities.Core.Contract.Integration.Accounts;

public interface IIdentityActiveAccountCheckerContract
{
    Task<Result<IdentityActiveAccountCheckResultDto>> CheckActiveAccountAsync(
        IdentityActiveAccountCheckRequestDto request,
        CancellationToken cancellationToken = default);
}
