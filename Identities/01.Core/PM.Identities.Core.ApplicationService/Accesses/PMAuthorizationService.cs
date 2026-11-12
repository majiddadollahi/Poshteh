using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using PM.Identities.Core.Contract.Accesses;
using PM.Identities.Core.Contract.Auth;
using PM.Identities.Core.Domain.Accesses;
using PM.Identities.Core.Domain.Users;
using PM.Identities.Infra.DAL.DbContext;
using PM.Procurement.Core.Contract.Integration.BuyerProfiles;
using PM.Procurement.Core.Contract.Integration.BuyerProfiles.DTOs;

namespace PM.Identities.Core.ApplicationService.Accesses;

/// <summary>
/// TASK-0247: central, deny-by-default authorization for every MVC endpoint. Registered scoped, so
/// the member state below is read at most once per request and never shared between requests.
/// </summary>
public sealed class PMAuthorizationService : IPMAuthorizationService
{
    private readonly IAdminAccessEvaluator _evaluator;
    private readonly PMIdentityDbContext _context;
    private readonly IBuyerProfileReaderContract _buyerProfiles;

    private MemberState? _member;
    private bool _buyerProfileLoaded;
    private BuyerProfileSummaryDto? _buyerProfile;

    public PMAuthorizationService(
        IAdminAccessEvaluator evaluator,
        PMIdentityDbContext context,
        IBuyerProfileReaderContract buyerProfiles)
    {
        _evaluator = evaluator;
        _context = context;
        _buyerProfiles = buyerProfiles;
    }

    public async Task<PMAuthorizationDecision> AuthorizeAsync(
        PMAuthorizationRequest request,
        CancellationToken cancellationToken = default)
    {
        var endpoint = await _evaluator.FindEndpointAsync(
            request.Application,
            request.HttpMethod,
            request.RoutePattern,
            cancellationToken);
        if (endpoint is null || !endpoint.Enable)
        {
            return PMAuthorizationDecision.Forbidden;
        }

        var panel = (AccessPanel)endpoint.Panel;
        var requirements = (AccessRequirements)endpoint.Requirements;
        if (panel == AccessPanel.Public)
        {
            return PMAuthorizationDecision.Allow;
        }

        // An admin Access without an admin rule is a catalog inconsistency, refused before
        // anything else — as the former filter did.
        if (panel == AccessPanel.Admin && request.AdminRule is null)
        {
            return PMAuthorizationDecision.Forbidden;
        }

        if (request.User.Identity?.IsAuthenticated != true)
        {
            return PMAuthorizationDecision.Unauthenticated;
        }

        return panel switch
        {
            AccessPanel.Shared => Decide(requirements == AccessRequirements.None),
            AccessPanel.Admin => await AuthorizeAdminAsync(request, endpoint.AccessId, requirements, cancellationToken),
            AccessPanel.Buyer => Decide(await IsBuyerAsync(request.User, requirements, cancellationToken)),
            AccessPanel.Vendor => Decide(await IsVendorAsync(request.User, requirements, cancellationToken)),
            AccessPanel.Marketplace => Decide(
                await IsVendorAsync(request.User, requirements, cancellationToken)
                || await IsBuyerAsync(request.User, requirements, cancellationToken)),
            _ => PMAuthorizationDecision.Forbidden
        };
    }

    public Task<bool> IsBuyerAccountAsync(ClaimsPrincipal user, CancellationToken cancellationToken = default) =>
        user.Identity?.IsAuthenticated == true
            ? IsBuyerAsync(user, AccessRequirements.None, cancellationToken)
            : Task.FromResult(false);

    private async Task<PMAuthorizationDecision> AuthorizeAdminAsync(
        PMAuthorizationRequest request,
        long accessId,
        AccessRequirements requirements,
        CancellationToken cancellationToken)
    {
        var user = request.User;
        var rule = request.AdminRule!;
        if (requirements != AccessRequirements.None)
        {
            return PMAuthorizationDecision.Forbidden;
        }

        // TASK-0044: AccountType is the necessary panel boundary, checked before any Role/Feature
        // evaluation — including the full-admin bypass below.
        if (!await _evaluator.HasPanelAccountTypeAsync(user, cancellationToken))
        {
            return PMAuthorizationDecision.Forbidden;
        }

        // R6: the full-admin role is honoured here, inside the admin panel only.
        if (await _evaluator.IsAdminAsync(user, cancellationToken))
        {
            return PMAuthorizationDecision.Allow;
        }

        if (rule.UsesOrganizationCode)
        {
            var code = request.ResolveOrganizationCode is null
                ? null
                : await request.ResolveOrganizationCode(cancellationToken);
            return Decide(!string.IsNullOrWhiteSpace(code)
                && await _evaluator.CanManageOrganizationAsync(user, code, cancellationToken));
        }

        if (rule.RequireAnyAdminPanelAccess)
        {
            return Decide(await _evaluator.HasAnyAdminPanelAccessAsync(user, cancellationToken));
        }

        if (rule.RequireOrganizationListAccess)
        {
            return Decide(await _evaluator.HasAnyOrganizationAccessAsync(user, cancellationToken)
                || await _evaluator.HasAccessAsync(user, AdminAccessOrganizationsKeys.CatalogCreate, cancellationToken));
        }

        if (rule.HasFeatureKeys
            && await _evaluator.HasAnyFeatureForAccessAsync(user, accessId, cancellationToken))
        {
            return PMAuthorizationDecision.Allow;
        }

        return Decide(rule.AllowAnyOrganizationAccess
            && await _evaluator.HasAnyOrganizationAccessAsync(user, cancellationToken));
    }

    /// <summary>
    /// Buyer panel (former BuyerAccount / ApprovedBuyer / ActiveBuyer): active user, buyer
    /// AccountType from the database and a real BuyerProfile. Approval and activity are read from
    /// the BuyerProfile in Procurement, never inferred from account metadata.
    /// </summary>
    private async Task<bool> IsBuyerAsync(
        ClaimsPrincipal user,
        AccessRequirements requirements,
        CancellationToken cancellationToken)
    {
        var member = await GetMemberAsync(user, cancellationToken);
        if (member is null || !member.AccountType.IsBuyer())
        {
            return false;
        }

        var profile = await GetBuyerProfileAsync(member.UserId, cancellationToken);
        if (profile is null)
        {
            return false;
        }

        if (requirements.HasFlag(AccessRequirements.ApprovedMember)
            && !(profile.IsActive && profile.Status == "Active"))
        {
            return false;
        }

        return !requirements.HasFlag(AccessRequirements.ActiveBuyer) || profile.IsActive;
    }

    /// <summary>
    /// Vendor panel (former VendorAccount / ApprovedVendor): active user, vendor AccountType and a
    /// VendorAccountKind; approval is <see cref="VendorReviewStatus.Active"/>.
    /// </summary>
    private async Task<bool> IsVendorAsync(
        ClaimsPrincipal user,
        AccessRequirements requirements,
        CancellationToken cancellationToken)
    {
        if (requirements.HasFlag(AccessRequirements.ActiveBuyer))
        {
            return false;
        }

        var member = await GetMemberAsync(user, cancellationToken);
        if (member is null || !member.AccountType.IsVendor() || member.VendorAccountKind is null)
        {
            return false;
        }

        return !requirements.HasFlag(AccessRequirements.ApprovedMember)
            || member.VendorReviewStatus == VendorReviewStatus.Active;
    }

    private async Task<MemberState?> GetMemberAsync(ClaimsPrincipal user, CancellationToken cancellationToken)
    {
        var userId = ResolveUserId(user);
        if (userId is null)
        {
            return null;
        }

        if (_member is not null && _member.UserId == userId.Value)
        {
            return _member.Exists ? _member : null;
        }

        var row = await _context.Users.AsNoTracking()
            .Where(u => u.Id == userId.Value && u.IsActive)
            .Select(u => new { u.AccountType, u.VendorAccountKind, u.VendorReviewStatus })
            .SingleOrDefaultAsync(cancellationToken);

        _member = row is null
            ? new MemberState(userId.Value, false, default, null, default)
            : new MemberState(userId.Value, true, row.AccountType, row.VendorAccountKind, row.VendorReviewStatus);
        _buyerProfileLoaded = false;
        _buyerProfile = null;
        return _member.Exists ? _member : null;
    }

    private async Task<BuyerProfileSummaryDto?> GetBuyerProfileAsync(long userId, CancellationToken cancellationToken)
    {
        if (!_buyerProfileLoaded)
        {
            _buyerProfile = await _buyerProfiles.GetByUserIdAsync(userId, cancellationToken);
            _buyerProfileLoaded = true;
        }

        return _buyerProfile;
    }

    private static PMAuthorizationDecision Decide(bool allowed) =>
        allowed ? PMAuthorizationDecision.Allow : PMAuthorizationDecision.Forbidden;

    private static long? ResolveUserId(ClaimsPrincipal user)
    {
        var raw = user.FindFirstValue(MahtaJwtClaimTypes.UserId)
            ?? user.FindFirstValue(ClaimTypes.NameIdentifier);
        return long.TryParse(raw, out var id) ? id : null;
    }

    private sealed record MemberState(
        long UserId,
        bool Exists,
        AccountType AccountType,
        VendorAccountKind? VendorAccountKind,
        VendorReviewStatus VendorReviewStatus);
}
