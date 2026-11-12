using PM.Identities.Core.Contract.Accesses.DTOs;
using Poshteh.Core.Domain;

namespace PM.Identities.Core.Contract.Accesses;

public interface IAdminAccessQueryService
{
    Task<Result<MyAdminAccessesDto>> GetMyAccessesAsync(
        long userId,
        CancellationToken cancellationToken = default);

    Task<Result<MyAdminAccessVersionDto>> GetMyAccessVersionAsync(
        long userId,
        CancellationToken cancellationToken = default);
}
