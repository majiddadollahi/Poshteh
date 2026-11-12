using PM.Identities.Core.Contract.Users.DTOs;
using PM.Identities.Core.Contract.Users.Queries;
using Poshteh.Core.ApplicationService.Queries;
using Poshteh.Core.Domain;
namespace PM.Identities.Core.ApplicationService.Users.Handlers;
public sealed class GetVendorOnboardingQueryHandler(VendorWorkflow workflow) : IQueryHandler<GetVendorOnboardingQuery, VendorOnboardingDto>
{
    public async Task<Result<VendorOnboardingDto>> Handle(GetVendorOnboardingQuery q, CancellationToken cancellationToken = default)
    {
        var u = await workflow.UserAsync(q.UserId, cancellationToken);
        return u is null ? Result.Failure<VendorOnboardingDto>(VendorProfileRules.Forbidden) : await workflow.DtoAsync(u, cancellationToken);
    }
}
