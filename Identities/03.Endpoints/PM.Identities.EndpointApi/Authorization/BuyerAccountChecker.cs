using System.Security.Claims;
using BuildingBlocks.Web.Authorization;
using PM.Identities.Core.Contract.Accesses;

namespace PM.Identities.EndpointApi.Authorization;

/// <summary>Adapts <see cref="IPMAuthorizationService.IsBuyerAccountAsync"/> to the web building block.</summary>
public sealed class BuyerAccountChecker(IPMAuthorizationService authorization) : IBuyerAccountChecker
{
    public Task<bool> IsBuyerAccountAsync(ClaimsPrincipal user, CancellationToken cancellationToken = default) =>
        authorization.IsBuyerAccountAsync(user, cancellationToken);
}
