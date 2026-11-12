using PM.Identities.Core.Contract.Features.DTOs;
using Poshteh.Core.Domain;

namespace PM.Identities.Core.Contract.Features;

public interface IRoleFeatureService
{
    Task<Result<List<FeatureDto>>> GetRoleFeaturesAsync(long roleId, CancellationToken cancellationToken = default);
    Task<Result<List<RoleFeatureSelectionDto>>> GetAllFeaturesWithSelectionAsync(long roleId, CancellationToken cancellationToken = default);
    Task<Result> AddFeatureToRoleAsync(long roleId, long featureId, CancellationToken cancellationToken = default);
    Task<Result> RemoveFeatureFromRoleAsync(long roleId, long featureId, CancellationToken cancellationToken = default);
    Task<Result> AddFeatureTreeToRoleAsync(long roleId, long featureId, CancellationToken cancellationToken = default);
    Task<Result> RemoveFeatureTreeFromRoleAsync(long roleId, long featureId, CancellationToken cancellationToken = default);
}
