using PM.Identities.Core.Contract.Accesses.DTOs;
using Poshteh.Core.Domain;

namespace PM.Identities.Core.Contract.Accesses;

public interface IAccessService
{
    Task<Result<AccessDto>> CreateAccessAsync(CreateAccessDto dto, CancellationToken cancellationToken = default);
    Task<Result<AccessDto>> UpdateAccessAsync(long id, UpdateAccessDto dto, CancellationToken cancellationToken = default);
    Task<Result> DeleteAccessAsync(long id, CancellationToken cancellationToken = default);
    Task<Result<AccessDto>> GetAccessByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<Result<AccessPageDto>> GetAccessListAsync(AccessListQuery query, CancellationToken cancellationToken = default);
    Task<Result<List<AccessResourceNodeDto>>> GetResourceTreeAsync(
        IReadOnlyCollection<EndpointAccessDefinitionDto> definitions,
        CancellationToken cancellationToken = default);
}
