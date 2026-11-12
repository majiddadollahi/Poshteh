using System.Security.Claims;

namespace PM.Identities.Core.Contract.Accesses;

/// <summary>Outcome of <see cref="IPMAuthorizationService.AuthorizeAsync"/>; mapped by the filter to 200/401/403.</summary>
public enum PMAuthorizationDecision
{
    Allow = 0,
    /// <summary>Not signed in on a non-public endpoint (401).</summary>
    Unauthenticated = 1,
    /// <summary>Unknown/disabled endpoint, wrong panel, or missing permission/state (403).</summary>
    Forbidden = 2
}

/// <summary>
/// Admin-only refinements of one action (from <c>AdminAccessRuleCatalog</c>), passed as data so the
/// decision logic stays in Identities while HTTP extraction stays in the filter.
/// </summary>
public sealed record AdminEndpointRuleDto(
    bool HasFeatureKeys,
    bool UsesOrganizationCode,
    bool RequireAnyAdminPanelAccess,
    bool RequireOrganizationListAccess,
    bool AllowAnyOrganizationAccess);

/// <summary>One authorization question for the current request (TASK-0247).</summary>
/// <param name="AdminRule">The action's admin rule, or <c>null</c> for non-admin actions.</param>
/// <param name="ResolveOrganizationCode">
/// Reads the organization code from the route/query/body; called only when the admin rule needs it.
/// </param>
public sealed record PMAuthorizationRequest(
    ClaimsPrincipal User,
    string Application,
    string HttpMethod,
    string RoutePattern,
    AdminEndpointRuleDto? AdminRule,
    Func<CancellationToken, Task<string?>>? ResolveOrganizationCode = null);

/// <summary>
/// TASK-0247: the single authorization decision for every MVC endpoint, driven by the persisted
/// Access registry (Panel + AccessMode + Requirements), the user's AccountType and Buyer/Vendor
/// state, and Role→Feature→Access grants for the admin panel. Deny by default.
/// </summary>
public interface IPMAuthorizationService
{
    Task<PMAuthorizationDecision> AuthorizeAsync(
        PMAuthorizationRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// True for an active user with a buyer AccountType and a real BuyerProfile (the former
    /// <c>BuyerAccount</c> policy) — for response shaping, not endpoint access.
    /// </summary>
    Task<bool> IsBuyerAccountAsync(ClaimsPrincipal user, CancellationToken cancellationToken = default);
}
