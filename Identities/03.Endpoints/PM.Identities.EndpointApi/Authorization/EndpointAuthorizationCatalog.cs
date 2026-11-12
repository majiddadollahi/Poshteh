using PM.Identities.Core.Domain.Accesses;

namespace PM.Identities.EndpointApi.Authorization;

/// <summary>Panel and member-state requirements of one controller action (TASK-0247).</summary>
public sealed record EndpointAuthorization(AccessPanel Panel, AccessRequirements Requirements);

/// <summary>
/// TASK-0247: the single, explicit source of every non-admin endpoint's panel and requirements,
/// replacing the former Authorize/AllowAnonymous/policy attributes. Admin endpoints are the
/// actions listed in <see cref="AdminAccessRuleCatalog"/>. Every MVC action must be classified
/// here or there — <see cref="Resolve"/> throws for anything else, so a new endpoint can never
/// silently become public. Entries are per action on purpose (no controller-wide default): a new
/// action in a known controller is denied until it is reviewed and listed.
/// <para>
/// The initial content was generated from the attributes it replaces and proven equal to them,
/// endpoint by endpoint, before those attributes were removed. Policy mapping:
/// VendorAccount → Vendor; ApprovedVendor → Vendor + ApprovedMember; ApprovedBuyer → Buyer +
/// ApprovedMember; ActiveBuyer → Buyer + ActiveBuyer; ApprovedMarketplaceMember → Marketplace +
/// ApprovedMember; plain Authorize → Shared; AllowAnonymous → Public.
/// </para>
/// </summary>
public static class EndpointAuthorizationCatalog
{
    private static readonly EndpointAuthorization AdminEndpoint = new(AccessPanel.Admin, AccessRequirements.None);

    private static readonly IReadOnlyDictionary<string, EndpointAuthorization> Entries = Build();

    /// <summary>Panel/requirements of an action, or <c>null</c> when it is not classified anywhere.</summary>
    public static EndpointAuthorization? Find(string controller, string action) =>
        AdminAccessRuleCatalog.Find(controller, action) is not null
            ? AdminEndpoint
            : Entries.TryGetValue($"{controller}.{action}", out var entry) ? entry : null;

    /// <summary>Like <see cref="Find"/>, but an unclassified action is a build/startup failure.</summary>
    public static EndpointAuthorization Resolve(string controller, string action) =>
        Find(controller, action)
        ?? throw new InvalidOperationException(
            $"Endpoint '{controller}.{action}' has no authorization definition. Add it to " +
            $"{nameof(EndpointAuthorizationCatalog)} (or {nameof(AdminAccessRuleCatalog)} for admin endpoints).");

    /// <summary>All non-admin entries, for inventory tests and reports.</summary>
    public static IReadOnlyDictionary<string, EndpointAuthorization> NonAdminEntries => Entries;

    private static IReadOnlyDictionary<string, EndpointAuthorization> Build()
    {
        var entries = new Dictionary<string, EndpointAuthorization>(StringComparer.Ordinal);

        void Add(AccessPanel panel, AccessRequirements requirements, params string[] actions)
        {
            foreach (var action in actions)
            {
                entries.Add(action, new EndpointAuthorization(panel, requirements));
            }
        }

        // Approved and active buyer (former ApprovedBuyer + ActiveBuyer).
        Add(AccessPanel.Buyer, AccessRequirements.ActiveBuyer | AccessRequirements.ApprovedMember,
            "BuyerCatalogItems.Create",
            "BuyerProfile.GetMyOrganizationUsers",
            "EquipmentPriceCalculation.Calculate",
            "Notifications.GetMyMessages",
            "PurchaseRequest.ApproveProposal",
            "PurchaseRequest.Cancel",
            "PurchaseRequest.Create",
            "PurchaseRequest.Edit",
            "PurchaseRequest.Extend",
            "PurchaseRequest.RejectProposal",
            "QuotaChargeRequest.Cancel",
            "QuotaChargeRequest.Create");

        // Approved buyer (former ApprovedBuyer).
        Add(AccessPanel.Buyer, AccessRequirements.ApprovedMember,
            "BuyerRequestQuota.GetMyQuota",
            "EquipmentPriceCalculation.GetMine",
            "PurchaseRequest.DownloadAttachment",
            "PurchaseRequest.DownloadProposalAttachmentForBuyer",
            "PurchaseRequest.GetById",
            "PurchaseRequest.GetHistory",
            "PurchaseRequest.GetMine",
            "PurchaseRequest.GetPriceCalculations",
            "PurchaseRequest.GetProposalReviewDetail",
            "PurchaseRequest.GetProposalsForReview");

        // Approved buyer or approved vendor (former ApprovedMarketplaceMember).
        Add(AccessPanel.Marketplace, AccessRequirements.ApprovedMember,
            "MasterDataRequests.GetMine",
            "MasterDataRequests.Submit");

        // Anonymous — the reviewed public surface (former AllowAnonymous, or no Authorize at all).
        Add(AccessPanel.Public, AccessRequirements.None,
            "ApiMetadata.GetByPrefix",
            "ApiMetadata.GetControllerMetadata",
            "ApiMetadata.GetIndex",
            "Auth.Login",
            "Auth.Logout",
            "Auth.RefreshToken",
            "Auth.RegisterBuyer",
            "Auth.RegisterVendor",
            "Users.RequestPasswordReset",
            "Version.Develop",
            "Version.Global");

        // Any authenticated user (former plain Authorize).
        Add(AccessPanel.Shared, AccessRequirements.None,
            "Brands.GetById",
            "Brands.GetList",
            "BuyerCeo.GetMine",
            "BuyerCeo.Register",
            "BuyerCeo.UpdateMine",
            "BuyerProfile.GetMyProfile",
            "BuyerProfile.ReviseMyProfile",
            "CatalogItems.GetAttributeOptions",
            "CatalogItems.GetById",
            "CatalogItems.GetList",
            "CategoryMeasures.GetList",
            "Classifications.GetById",
            "Classifications.GetChildren",
            "Classifications.GetTree",
            "Countries.GetById",
            "Countries.GetList",
            "MyAdminAccesses.ChangeMyPassword",
            "MyAdminAccesses.GetMyAccessVersion",
            "MyAdminAccesses.GetMyAccesses",
            "MyAdminAccesses.GetMyProfile",
            "MyAdminAccesses.UpdateMyProfile",
            "Notifications.GetMine",
            "Notifications.GetPendingCount",
            "Notifications.GetUnreadCount",
            "Notifications.MarkAllAsRead",
            "Notifications.MarkAsRead",
            "PriceEstimation.GetMine",
            "QuotaChargeRequest.GetMine",
            "QuotaChargeRequest.GetMyTimeline",
            "QuotaChargeRequest.GetUnitPrice",
            "UnitMeasures.GetById",
            "UnitMeasures.GetList");

        // Approved vendor (former ApprovedVendor).
        Add(AccessPanel.Vendor, AccessRequirements.ApprovedMember,
            "PurchaseRequest.CreateOrGetProposalDraft",
            "PurchaseRequest.DeclinePublishedInvitation",
            "PurchaseRequest.DownloadProposalProforma",
            "PurchaseRequest.DownloadPublishedAttachment",
            "PurchaseRequest.DownloadTechnicalProposal",
            "PurchaseRequest.GetProposalItems",
            "PurchaseRequest.GetProposalProforma",
            "PurchaseRequest.GetPublishedToMe",
            "PurchaseRequest.GetPublishedToMeById",
            "PurchaseRequest.GetTechnicalProposal",
            "PurchaseRequest.RemoveProposalProforma",
            "PurchaseRequest.RemoveTechnicalProposal",
            "PurchaseRequest.SaveProposalItems",
            "PurchaseRequest.SubmitProposal",
            "PurchaseRequest.UploadProposalProforma",
            "PurchaseRequest.UploadTechnicalProposal",
            "VendorActivityDeclaration.Create",
            "VendorActivityDeclaration.GetMine");

        // Vendor account, approval not required (former VendorAccount).
        Add(AccessPanel.Vendor, AccessRequirements.None,
            "CompanyProfile.GetMyProfile",
            "VendorActivityWorkspace.Get",
            "VendorActivityWorkspace.Save",
            "VendorOnboarding.Get",
            "VendorOnboarding.Submit",
            "VendorOnboarding.Update");

        return entries;
    }
}
