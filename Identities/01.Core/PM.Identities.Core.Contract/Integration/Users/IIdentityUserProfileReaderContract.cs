using PM.Identities.Core.Contract.Integration.Users.DTOs;
using Poshteh.Core.Domain;

namespace PM.Identities.Core.Contract.Integration.Users;

public interface IIdentityUserProfileReaderContract
{
    Task<Result<IdentityUserProfileDto>> GetUserProfileAsync(
        long userId,
        CancellationToken cancellationToken = default);
}
