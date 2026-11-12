using PM.Identities.Core.Contract.Features.DTOs;
using Poshteh.Core.Domain;

namespace PM.Identities.Core.Contract.Features;

public interface IFeatureService
{
    Task<Result<FeatureDto>> CreateFeatureAsync(CreateFeatureDto dto, CancellationToken cancellationToken = default);
    Task<Result<FeatureDto>> UpdateFeatureAsync(long id, UpdateFeatureDto dto, CancellationToken cancellationToken = default);
    Task<Result> DeleteFeatureAsync(long id, CancellationToken cancellationToken = default);
    Task<Result<FeatureDto>> GetFeatureByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<Result<List<FeatureDto>>> GetFeatureListAsync(CancellationToken cancellationToken = default);
    Task<Result<List<FeatureTreeNodeDto>>> GetFeatureTreeAsync(CancellationToken cancellationToken = default);
    Task<Result<List<FeatureLinkedAccessDto>>> GetFeatureAccessesAsync(long featureId, CancellationToken cancellationToken = default);
    Task<Result> ReplaceFeatureAccessesAsync(long featureId, ReplaceFeatureAccessesDto dto, CancellationToken cancellationToken = default);
}
