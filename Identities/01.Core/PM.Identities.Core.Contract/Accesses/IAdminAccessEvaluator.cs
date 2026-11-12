using System.Security.Claims;
using PM.Identities.Core.Contract.Accesses.DTOs;

namespace PM.Identities.Core.Contract.Accesses;

public interface IAdminAccessEvaluator
{
    /// <summary>
    /// TASK-0044: true when the user's AccountType (read from the database, never trusted from a
    /// claim) belongs to the admin/Panel family. This is the necessary panel-membership boundary;
    /// it is checked before any Role/access-key evaluation, which only governs capabilities
    /// *inside* the panel for an account that already belongs to it.
    /// </summary>
    Task<bool> HasPanelAccountTypeAsync(ClaimsPrincipal user, CancellationToken cancellationToken = default);

    Task<bool> IsAdminAsync(ClaimsPrincipal user, CancellationToken cancellationToken = default);

    Task<bool> HasAccessAsync(ClaimsPrincipal user, string key, CancellationToken cancellationToken = default);

    Task<bool> HasAnyAccessAsync(
        ClaimsPrincipal user,
        IEnumerable<string> keys,
        CancellationToken cancellationToken = default);

    Task<EndpointAccessAuthorizationDto?> FindEndpointAsync(
        string application,
        string httpMethod,
        string routePattern,
        CancellationToken cancellationToken = default);

    Task<bool> HasAnyFeatureForAccessAsync(
        ClaimsPrincipal user,
        long accessId,
        CancellationToken cancellationToken = default);

    /// <summary>True when the user has any Access key under the Admin.* catalog.</summary>
    Task<bool> HasAnyAdminPanelAccessAsync(ClaimsPrincipal user, CancellationToken cancellationToken = default);

    Task<bool> CanManageOrganizationAsync(
        ClaimsPrincipal user,
        string tenantCode,
        CancellationToken cancellationToken = default);

    Task<bool> HasAnyOrganizationAccessAsync(
        ClaimsPrincipal user,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<string>> GetAccessKeysAsync(
        ClaimsPrincipal user,
        CancellationToken cancellationToken = default);

    /// <summary>Tenant codes the user may manage in admin panel (from organization access keys).</summary>
    Task<IReadOnlyCollection<string>> GetManagedOrganizationCodesAsync(
        ClaimsPrincipal user,
        CancellationToken cancellationToken = default);
}
