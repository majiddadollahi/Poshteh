using PM.Identities.Core.Contract.Accesses;

namespace PM.Identities.EndpointApi.Authorization;

/// <summary>
/// Central catalog of Admin Panel access rules.
/// Controllers stay free of authorization attributes; this is the single source of truth.
/// </summary>
public static class AdminAccessRuleCatalog
{
    private static readonly Dictionary<string, AdminAccessRule> Rules =
        Build().ToDictionary(r => NormalizeKey(r.Controller, r.Action), StringComparer.Ordinal);

    public static AdminAccessRule? Find(string controllerName, string actionName)
    {
        Rules.TryGetValue(NormalizeKey(controllerName, actionName), out var rule);
        return rule;
    }

    /// <summary>True when this controller/action is covered by the admin-panel access catalog.</summary>
    public static bool IsProtected(string controllerName, string actionName) =>
        Find(controllerName, actionName) is not null;

    public static IReadOnlyCollection<AdminAccessRule> All => Rules.Values;

    private static string NormalizeKey(string controller, string action) =>
        $"{controller.Trim()}/{action.Trim()}".ToUpperInvariant();

    private static IEnumerable<AdminAccessRule> Build()
    {
        yield return Key("VendorActivityReview", "Review", "Admin.VendorDeclarations.Activity.Decide");
        yield return Key("VendorActivityDeclaration", "GetList", "Admin.VendorDeclarations.Activity.View");
        yield return Key("VendorActivityDeclaration", "Approve", "Admin.VendorDeclarations.Activity.Decide");
        yield return Key("VendorActivityDeclaration", "Reject", "Admin.VendorDeclarations.Activity.Decide");
        yield return Key("Vendors", "GetList", AdminAccessVendorsKeys.View);
        yield return Key("Vendors", "GetById", AdminAccessVendorsKeys.View);
        yield return Key("Vendors", "Approve", AdminAccessVendorsKeys.Review);
        yield return Key("Vendors", "Reject", AdminAccessVendorsKeys.Review);
        yield return Key("Vendors", "RequestCorrection", AdminAccessVendorsKeys.Review);
        // ── تأمین‌کننده (فعال/غیرفعال‌سازی توسط مدیر سامانه — بخشی از همان lifecycle واحد
        // VendorReviewStatus، TASK-0020). فقط اکشن‌های مدیر سامانه؛ CompanyProfile.GetMyProfile
        // مربوط به تأمین‌کننده خودش است و اینجا ثبت نمی‌شود. ──
        yield return Key("Vendors", "Activate", AdminAccessVendorsKeys.Activate);
        yield return Key("Vendors", "Deactivate", AdminAccessVendorsKeys.Deactivate);
        // â”€â”€ Users â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        yield return Key("Users", "GetList", AdminAccessUsersKeys.View, AdminAccessRolesKeys.ManageUsers);
        yield return Key("Users", "GetManagementList", AdminAccessUsersKeys.View);
        yield return new AdminAccessRule
        {
            Controller = "Users",
            Action = "Search",
            Keys = [AdminAccessUsersKeys.View],
            AllowAnyOrganizationAccess = true
        };
        yield return Key("Users", "GetById", AdminAccessUsersKeys.View, AdminAccessUsersKeys.ManageRoles);
        yield return Key("Users", "GetUserRoles", AdminAccessUsersKeys.ManageRoles);
        yield return Key("Users", "AddRoleToUser", AdminAccessUsersKeys.ManageRoles);
        yield return Key("Users", "RemoveRoleFromUser", AdminAccessUsersKeys.ManageRoles);
        yield return Key("Users", "Create", AdminAccessUsersKeys.Create);
        yield return Key("Users", "Update", AdminAccessUsersKeys.Update);
        yield return Key("Users", "Delete", AdminAccessUsersKeys.Update);
        yield return Key("Users", "ChangePassword", AdminAccessUsersKeys.ResetPassword);
        yield return Key("Users", "AdminResetPassword", AdminAccessUsersKeys.ResetPassword);
        yield return Key("Users", "Activate", AdminAccessUsersKeys.Activate);
        yield return Key("Users", "Deactivate", AdminAccessUsersKeys.Deactivate);

        // â”€â”€ Roles â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        yield return Key("Roles", "GetList", AdminAccessRolesKeys.View, AdminAccessUsersKeys.ManageRoles);
        yield return Key("Roles", "GetById", AdminAccessRolesKeys.View,
            AdminAccessRolesKeys.ManageUsers, AdminAccessRolesKeys.ManageAccesses);
        yield return Key("Roles", "Create", AdminAccessRolesKeys.Create);
        yield return Key("Roles", "Update", AdminAccessRolesKeys.Update);
        yield return Key("Roles", "Delete", AdminAccessRolesKeys.Update);
        yield return Key("Roles", "Activate", AdminAccessRolesKeys.Activate);
        yield return Key("Roles", "Deactivate", AdminAccessRolesKeys.Deactivate);
        yield return Key("Roles", "AddUserToRole", AdminAccessRolesKeys.ManageUsers);
        yield return Key("Roles", "RemoveUserFromRole", AdminAccessRolesKeys.ManageUsers);
        yield return Key("Roles", "GetRoleUsers", AdminAccessRolesKeys.ManageUsers);
        yield return new AdminAccessRule
        {
            Controller = "Roles",
            Action = "GetUserRoles",
            Keys = [AdminAccessUsersKeys.ManageRoles, AdminAccessRolesKeys.ManageUsers]
        };

        // Access resources
        yield return Key("Access", "GetList", AdminAccessPermissionsKeys.View);
        yield return Key("Access", "GetTree", AdminAccessPermissionsKeys.View);
        yield return Key("Access", "GetById", AdminAccessPermissionsKeys.View);
        yield return Key("Access", "Create", AdminAccessPermissionsKeys.Create);
        yield return Key("Access", "Update", AdminAccessPermissionsKeys.Update);
        yield return Key("Access", "Delete", AdminAccessPermissionsKeys.Delete);

        // Features
        yield return Key("Feature", "GetList", AdminAccessPermissionsKeys.View);
        yield return Key("Feature", "GetTree", AdminAccessPermissionsKeys.View);
        yield return Key("Feature", "GetById", AdminAccessPermissionsKeys.View);
        yield return Key("Feature", "GetAccesses", AdminAccessPermissionsKeys.View);
        yield return Key("Feature", "Create", AdminAccessPermissionsKeys.Create);
        yield return Key("Feature", "Update", AdminAccessPermissionsKeys.Update);
        yield return Key("Feature", "ReplaceAccesses", AdminAccessPermissionsKeys.Update);
        yield return Key("Feature", "Delete", AdminAccessPermissionsKeys.Delete);

        // RoleFeature
        yield return Key("RoleFeature", "GetRoleFeatures", AdminAccessRolesKeys.ManageAccesses);
        yield return Key("RoleFeature", "GetAllFeaturesWithSelection", AdminAccessRolesKeys.ManageAccesses);
        yield return Key("RoleFeature", "RemoveFeature", AdminAccessRolesKeys.ManageAccesses);
        yield return Key("RoleFeature", "AddFeature", AdminAccessRolesKeys.ManageAccesses);
        yield return Key("RoleFeature", "AddFeatureTree", AdminAccessRolesKeys.ManageAccesses);
        yield return Key("RoleFeature", "RemoveFeatureTree", AdminAccessRolesKeys.ManageAccesses);

        // â”€â”€ Tenant (admin panel; GetMyList is intentionally not listed) â”€â”€
        yield return new AdminAccessRule
        {
            Controller = "Tenant",
            Action = "GetList",
            RequireOrganizationListAccess = true
        };
        yield return Key("Tenant", "Create", AdminAccessOrganizationsKeys.CatalogCreate);
        yield return OrgRoute("Tenant", "GetByCode", "code");
        yield return OrgRoute("Tenant", "Update", "code");
        yield return OrgRoute("Tenant", "Activate", "code");
        yield return OrgRoute("Tenant", "Deactivate", "code");
        yield return OrgRoute("Tenant", "Delete", "code");
        yield return OrgRoute("Tenant", "GetMembers", "code");
        yield return OrgRoute("Tenant", "AddMember", "code");
        yield return OrgRoute("Tenant", "UpdateMember", "code");
        yield return OrgRoute("Tenant", "RemoveMember", "code");

        // â”€â”€ QC â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        yield return Key("QcDashboard", "GetStats", AdminAccessQcKeys.DashboardView);
        yield return Key("QcDashboard", "GetFullDashboard", AdminAccessQcKeys.DashboardView);
        yield return Key("QcRecord", "Create", AdminAccessQcKeys.RecordsCreate);
        yield return Key("QcRecord", "Update", AdminAccessQcKeys.RecordsUpdate);
        yield return Key("QcRecord", "Delete", AdminAccessQcKeys.RecordsDelete);
        yield return Key("QcRecord", "GetById", AdminAccessQcKeys.RecordsView);
        yield return Key("QcRecord", "GetPagedList", AdminAccessQcKeys.RecordsView);
        yield return Key("QcRecord", "Search", AdminAccessQcKeys.RecordsView);

        // ── MasterDataRequests (افزوده‌شده 2026/09/10 — BR-44 تا BR-47) ──────
        yield return Key("MasterDataRequests", "GetList", "Admin.MasterDataRequests.View");
        yield return Key("MasterDataRequests", "GetPendingCounts", "Admin.MasterDataRequests.View");
        yield return Key("MasterDataRequests", "GetById", "Admin.MasterDataRequests.View");
        yield return Key("MasterDataRequests", "Approve", "Admin.MasterDataRequests.Decide");
        yield return Key("MasterDataRequests", "Reject", "Admin.MasterDataRequests.Decide");

        // ── PurchaseRequests (افزوده‌شده 2026/09/12 — بررسی/تأیید/رد/انتشار توسط مدیر سامانه).
        // فقط اکشن‌های مختص مدیر سامانه؛ Create/GetMine/GetById/Edit/Cancel/Extend مربوط به خریدار مالک‌اند و اینجا ثبت نمی‌شوند. ──
        yield return Key("PurchaseRequest", "GetList", "Admin.PurchaseRequests.View");
        yield return Key("PurchaseRequest", "GetAdminSummary", "Admin.PurchaseRequests.View");
        yield return Key("PurchaseRequest", "GetByIdForAdmin", "Admin.PurchaseRequests.View",
            "Admin.PurchaseRequests.Publish");
        yield return Key("PurchaseRequest", "GetHistoryForAdmin", "Admin.PurchaseRequests.View");
        yield return Key("PurchaseRequest", "GetPublicationsForAdmin", "Admin.PurchaseRequests.View");
        yield return Key("PurchaseRequest", "GetInvitationRosterForAdmin", "Admin.PurchaseRequests.View");
        yield return Key("PurchaseRequest", "GetProposalSummaryForAdmin", "Admin.PurchaseRequests.View");
        yield return Key("PurchaseRequest", "GetProposalDetailForAdmin", "Admin.PurchaseRequests.View");
        yield return Key("PurchaseRequest", "DownloadProposalProformaForAdmin", "Admin.PurchaseRequests.View");
        yield return Key("PurchaseRequest", "DownloadProposalAttachmentForAdmin", "Admin.PurchaseRequests.View");
        yield return Key("PurchaseRequest", "DownloadAttachmentForAdmin", "Admin.PurchaseRequests.View");
        yield return Key("PurchaseRequest", "Approve", "Admin.PurchaseRequests.Decide");
        yield return Key("PurchaseRequest", "Reject", "Admin.PurchaseRequests.Decide");
        yield return Key("PurchaseRequest", "RequestRevision", "Admin.PurchaseRequests.Decide");
        yield return Key("PurchaseRequest", "SearchSupplierCandidates", "Admin.PurchaseRequests.Publish");
        yield return Key("PurchaseRequest", "Publish", "Admin.PurchaseRequests.Publish");
        yield return Key("PurchaseRequest", "CloseProposalCollection", "Admin.PurchaseRequests.Publish");
        yield return Key("PurchaseRequest", "FinalizePrices", "Admin.PurchaseRequests.Decide");
        yield return Key("PriceEstimation", "GetForRequest", "Admin.PurchaseRequests.View");
        yield return Key("PriceEstimation", "Create", "Admin.PurchaseRequests.Decide");
        yield return Key("PriceEstimation", "Revise", "Admin.PurchaseRequests.Decide");
        yield return Key("PriceEstimation", "Finalize", "Admin.PurchaseRequests.Decide");

        // Dashboard widgets are independent endpoints but expose the same Procurement scope.
        yield return Key("AdminDashboard", "GetPurchaseRequestCount", "Admin.PurchaseRequests.View");
        yield return Key("AdminDashboard", "GetAdminActionCount", "Admin.PurchaseRequests.View");
        yield return Key("AdminDashboard", "GetPublishedCount", "Admin.PurchaseRequests.View");
        yield return Key("AdminDashboard", "GetSupplierOfferCount", "Admin.PurchaseRequests.View");
        yield return Key("AdminDashboard", "GetMonthlyTrend", "Admin.PurchaseRequests.View");
        yield return Key("AdminDashboard", "GetStatusDistribution", "Admin.PurchaseRequests.View");
        yield return Key("AdminDashboard", "GetRecentPurchaseRequests", "Admin.PurchaseRequests.View");
        yield return Key("AdminDashboard", "GetRecentSuppliers", "Admin.PurchaseRequests.View");
        yield return Key("AdminDashboard", "GetRecentOffers", "Admin.PurchaseRequests.View");

        // Capacity administration. Buyer-owned endpoints (me/mine/create/cancel) are intentionally
        // excluded and continue to use their buyer policies.
        yield return Key("BuyerRequestQuota", "GetByBuyerUserId", "Admin.Capacity.Quota.View");
        yield return Key("BuyerRequestQuota", "SetQuota", "Admin.Capacity.Quota.Manage");
        yield return Key("QuotaChargeRequest", "GetList", "Admin.Capacity.ChargeRequests.View");
        yield return Key("QuotaChargeRequest", "GetTimelineForAdmin", "Admin.Capacity.Quota.View");
        yield return Key("QuotaChargeRequest", "Approve", "Admin.Capacity.ChargeRequests.Decide");
        yield return Key("QuotaChargeRequest", "Reject", "Admin.Capacity.ChargeRequests.Decide");
        yield return Key("EquipmentPriceCalculation", "GetList", "Admin.Equipment.View");

        // Master-data administration. Read-only catalog endpoints remain available to authenticated
        // business users; every mutation requires its dedicated admin permission.
        yield return Key("CatalogItems", "Create", "Admin.MasterData.CatalogItems.Create");
        yield return Key("CatalogItems", "ValidateImport", "Admin.MasterData.CatalogItems.Create");
        yield return Key("CatalogItems", "CommitImport", "Admin.MasterData.CatalogItems.Create");
        yield return Key("CatalogItems", "Update", "Admin.MasterData.CatalogItems.Update");
        yield return Key("CatalogItems", "Delete", "Admin.MasterData.CatalogItems.Delete");
        yield return Key("Classifications", "Create", "Admin.MasterData.Classifications.Create");
        yield return Key("Classifications", "Update", "Admin.MasterData.Classifications.Update");
        yield return Key("Classifications", "Delete", "Admin.MasterData.Classifications.Delete");
        yield return Key("Classifications", "ValidateImport", "Admin.MasterData.Classifications.Create");
        yield return Key("Classifications", "CommitImport", "Admin.MasterData.Classifications.Create");
        // ClassificationAttribute (TASK-0085): reuses the existing Update permission — no dedicated
        // key, since this is an admin-only management action on the Classification Tree page.
        yield return Key("Classifications", "GetAttributes", "Admin.MasterData.Classifications.Update");
        yield return Key("Classifications", "SetAttributes", "Admin.MasterData.Classifications.Update");
        yield return Key("Countries", "Create", "Admin.MasterData.Countries.Create");
        yield return Key("Countries", "Update", "Admin.MasterData.Countries.Update");
        yield return Key("Countries", "Activate", "Admin.MasterData.Countries.Update");
        yield return Key("Countries", "Deactivate", "Admin.MasterData.Countries.Update");
        yield return Key("UnitMeasures", "Create", "Admin.MasterData.UnitMeasures.Create");
        yield return Key("UnitMeasures", "Update", "Admin.MasterData.UnitMeasures.Update");
        yield return Key("UnitMeasures", "Delete", "Admin.MasterData.UnitMeasures.Delete");
        yield return Key("Brands", "Create", "Admin.MasterData.Brands.Create");
        yield return Key("Brands", "Update", "Admin.MasterData.Brands.Update");
        yield return Key("Brands", "Delete", "Admin.MasterData.Brands.Delete");
        // ProductAttributes (TASK-0081): admin-only management with no business consumer yet, so
        // unlike the other Catalogs master-data controllers, reads are also gated by View.
        yield return Key("AttributeDefinitions", "GetList", "Admin.MasterData.ProductAttributes.View");
        yield return Key("AttributeDefinitions", "GetById", "Admin.MasterData.ProductAttributes.View");
        yield return Key("AttributeDefinitions", "GetValues", "Admin.MasterData.ProductAttributes.View");
        yield return Key("AttributeDefinitions", "Create", "Admin.MasterData.ProductAttributes.Create");
        yield return Key("AttributeDefinitions", "CreateValue", "Admin.MasterData.ProductAttributes.Create");
        yield return Key("AttributeDefinitions", "Update", "Admin.MasterData.ProductAttributes.Update");
        yield return Key("AttributeDefinitions", "UpdateValue", "Admin.MasterData.ProductAttributes.Update");
        yield return Key("AttributeDefinitions", "ActivateValue", "Admin.MasterData.ProductAttributes.Update");
        yield return Key("AttributeDefinitions", "DeactivateValue", "Admin.MasterData.ProductAttributes.Update");

        // ── Buyers (فعال/غیرفعال‌سازی خریدار توسط مدیر سامانه، مستقل از VendorReviewStatus).
        // فقط اکشن‌های مدیر سامانه؛ BuyerProfile.GetMyProfile/BuyerCeo.GetMine/Register مربوط به
        // خریدار خودش‌اند و اینجا ثبت نمی‌شوند. ──
        yield return Key("BuyerProfile", "GetList", AdminAccessBuyersKeys.View);
        yield return Key("BuyerProfile", "GetById", AdminAccessBuyersKeys.View);
        yield return Key("BuyerProfile", "GetUsersForAdmin", AdminAccessBuyersKeys.View);
        yield return Key("BuyerProfile", "GetMessagesForAdmin", AdminAccessNotificationsKeys.View);
        yield return Key("BuyerProfile", "Activate", AdminAccessBuyersKeys.Activate);
        yield return Key("BuyerProfile", "Deactivate", AdminAccessBuyersKeys.Deactivate);
        yield return Key("BuyerProfile", "RequestRevision", AdminAccessBuyersKeys.Activate);
        yield return Key("BuyerCeo", "GetByBuyerProfileId", AdminAccessBuyersKeys.View);

        // ── Settings (BR-26: همیشه فقط مدیر سامانه، جزو مجوزهای قابل‌تفویض به اپراتور نیست —
        // بدون Keys یعنی فقط IsAdminAsync مجاز است، حتی اپراتور با دسترسی کامل هم Forbidden می‌شود) ──
        yield return Key("Settings", "GetSettings");
        yield return Key("Settings", "GetById");
        yield return Key("Settings", "GetHistory");
        yield return Key("Settings", "Create");
        yield return Key("Settings", "Update");

        // ── TestManagement (TASK-0121/TASK-0124): قابلیت مستقل مدیریت تست، فقط System Admin. ──
        yield return Key("TestManagement", "GetList");
        yield return Key("TestManagement", "GetTodayStats");
        yield return Key("TestManagement", "GetHistory");
        yield return Key("TestManagement", "Create");
        yield return Key("TestManagement", "Update");
        yield return Key("TestManagement", "Delete");
        yield return Key("TestManagement", "RecordExecution");

        // ── Notifications (فهرست سراسری اعلان‌ها برای مدیر سامانه؛ اکشن‌های کاربر جاری
        // GetPendingCount/GetUnreadCount/GetMine/MarkAsRead/MarkAllAsRead روی همین Controller مربوط
        // به اعلان‌های خودِ کاربرند و اینجا ثبت نمی‌شوند) ──
        yield return Key("Notifications", "GetAllForAdmin", AdminAccessNotificationsKeys.View);
        yield return Key("Notifications", "GetEventCodes", AdminAccessNotificationsKeys.View);

        // ── SentMessages (لاگ پیامک/ایمیل واقعاً ارسال‌شدهٔ سامانه — BL-007، بخش ۴ پرامپت توسعه) ──
        yield return Key("SentMessages", "GetSms", AdminAccessSentMessagesKeys.View);
        yield return Key("SentMessages", "GetEmail", AdminAccessSentMessagesKeys.View);

        // ── AdminWork (TASK-0061): شمارندهٔ نیازمند اقدام؛ ورود با هر یک از کلیدهای View، و هر حوزه داخل Action با کلید View خودش فیلتر می‌شود ──
        yield return Key("VendorAdminWork", "GetPendingBySupplier", AdminAccessVendorsKeys.View,
            "Admin.VendorDeclarations.Activity.View");
        yield return Key("AdminWork", "GetSummary", AdminAccessBuyersKeys.View, "Admin.PurchaseRequests.View",
            "Admin.Capacity.ChargeRequests.View", AdminAccessVendorsKeys.View,
            "Admin.VendorDeclarations.Activity.View");
    }

    private static AdminAccessRule Key(string controller, string action, params string[] keys) =>
        new()
        {
            Controller = controller,
            Action = action,
            Keys = keys
        };

    private static AdminAccessRule OrgRoute(string controller, string action, string routeParameter) =>
        new()
        {
            Controller = controller,
            Action = action,
            OrganizationRouteParameter = routeParameter
        };
}
