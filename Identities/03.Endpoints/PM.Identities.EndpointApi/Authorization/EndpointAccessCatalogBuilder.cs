using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using PM.Identities.Core.Contract.Accesses;
using PM.Identities.Core.Contract.Accesses.DTOs;
using PM.Identities.Core.Domain.Accesses;

namespace PM.Identities.EndpointApi.Authorization;

/// <summary>
/// Creates the managed Access catalog from the same MVC metadata used for routing.
/// Generated OpenAPI/Scalar endpoints are deliberately excluded because they are not controller APIs.
/// </summary>
public static class EndpointAccessCatalogBuilder
{
    private static readonly IReadOnlyDictionary<string, string> ExplicitKeys =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["POST /vendor/Onboarding/submit-for-approval"] = "identities.vendoronboarding.submit",
            ["POST /vendor/Onboarding/resubmit"] = "identities.vendoronboarding.resubmit"
        };

    // TASK-0181: Persian titles for the module-grouping level of the Access resource tree
    // (/identity/Access/tree). Module keys are lowercase, resolved by ResolveModule from the
    // controller's assembly name — keep this in sync with EndpointAccessCatalogTests' six module
    // counts (host/identities/catalogs/platform/procurement/vendor).
    private static readonly IReadOnlyDictionary<string, string> ModuleTitles =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["host"] = "زیرساخت سامانه",
            ["identities"] = "هویت و دسترسی",
            ["catalogs"] = "کاتالوگ",
            ["platform"] = "پلتفرم",
            ["procurement"] = "تدارکات",
            ["vendor"] = "تامین‌کنندگان"
        };

    private static readonly IReadOnlyDictionary<string, string> ResourceTitles =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["Access"] = "دسترسی‌ها", ["AdminDashboard"] = "داشبورد مدیریتی", ["AdminWork"] = "کارهای مدیریتی",
            ["ApiMetadata"] = "فراداده رابط برنامه‌نویسی", ["AttributeDefinitions"] = "ویژگی‌های فنی",
            ["Auth"] = "احراز هویت", ["Brands"] = "برندها",
            ["BuyerCatalogItems"] = "اقلام کاتالوگ خریدار", ["BuyerCeo"] = "مدیرعامل خریدار",
            ["BuyerProfile"] = "پروفایل خریدار", ["BuyerRequestQuota"] = "سهمیه درخواست خریدار",
            ["CatalogItems"] = "اقلام کاتالوگ", ["CategoryMeasures"] = "واحدهای اندازه‌گیری دسته‌بندی",
            ["Classifications"] = "طبقه‌بندی‌ها", ["CompanyProfile"] = "پروفایل شرکت",
            ["Countries"] = "کشورها",
            ["EquipmentPriceCalculation"] = "محاسبه قیمت تجهیزات", ["Feature"] = "قابلیت‌ها",
            ["MasterDataRequests"] = "درخواست‌های داده پایه", ["MyAdminAccesses"] = "حساب مدیریتی من",
            ["Notifications"] = "اعلان‌ها", ["PriceEstimation"] = "برآورد قیمت",
            ["PurchaseRequest"] = "درخواست خرید",
            ["QuotaChargeRequest"] = "درخواست شارژ سهمیه", ["RoleFeature"] = "قابلیت‌های نقش",
            ["Roles"] = "نقش‌ها", ["SentMessages"] = "پیام‌های ارسال‌شده",
            ["Settings"] = "تنظیمات", ["TestManagement"] = "مدیریت آزمون‌ها",
            ["UnitMeasures"] = "واحدهای اندازه‌گیری", ["Users"] = "کاربران",
            ["VendorActivityDeclaration"] = "اظهار فعالیت تامین‌کننده",
            ["VendorActivityReview"] = "بررسی فعالیت تامین‌کننده",
            ["VendorActivityWorkspace"] = "فضای کاری فعالیت تامین‌کننده",
            ["VendorAdminWork"] = "کارهای مدیریتی تامین‌کننده",
            ["VendorOnboarding"] = "پذیرش تامین‌کننده", ["Vendors"] = "تامین‌کنندگان",
            ["Version"] = "نسخه سامانه"
        };

    // Titles that cannot be expressed accurately by combining an action verb with the
    // controller resource. Keeping these reviewed exceptions explicit prevents vague titles
    // such as "مشاهده درخواست خرید" for proposal, attachment, history, and invitation APIs.
    private static readonly IReadOnlyDictionary<string, string> ExplicitPersianTitles =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["POST /vendor/onboarding/submit-for-approval"] = "ارسال پرونده تامین‌کننده برای بررسی",
            ["POST /vendor/onboarding/resubmit"] = "ارسال مجدد پرونده اصلاح‌شده تامین‌کننده",

            ["ApiMetadata.GetByPrefix"] = "مشاهده فراداده API بر اساس پیشوند",
            ["ApiMetadata.GetControllerMetadata"] = "مشاهده فراداده Controller",
            ["ApiMetadata.GetIndex"] = "مشاهده فهرست فراداده APIها",
            ["Auth.Login"] = "ورود به سامانه",
            ["Auth.Logout"] = "خروج از سامانه",
            ["Auth.RefreshToken"] = "تمدید نشست کاربر",
            ["Auth.RegisterBuyer"] = "ثبت‌نام خریدار",
            ["Auth.RegisterVendor"] = "ثبت‌نام تامین‌کننده",

            ["AdminDashboard.GetPurchaseRequestCount"] = "مشاهده تعداد کل درخواست‌های خرید داشبورد",
            ["AdminDashboard.GetAdminActionCount"] = "مشاهده درخواست‌های نیازمند اقدام مدیر داشبورد",
            ["AdminDashboard.GetPublishedCount"] = "مشاهده درخواست‌های منتشرشده داشبورد",
            ["AdminDashboard.GetSupplierOfferCount"] = "مشاهده تعداد پیشنهادهای تامین‌کنندگان داشبورد",
            ["AdminDashboard.GetMonthlyTrend"] = "مشاهده روند شش‌ماهه درخواست‌های خرید داشبورد",
            ["AdminDashboard.GetStatusDistribution"] = "مشاهده توزیع وضعیت درخواست‌های خرید داشبورد",
            ["AdminDashboard.GetRecentPurchaseRequests"] = "مشاهده آخرین درخواست‌های خرید داشبورد",
            ["AdminDashboard.GetRecentSuppliers"] = "مشاهده آخرین فعالیت تامین‌کنندگان داشبورد",
            ["AdminDashboard.GetRecentOffers"] = "مشاهده آخرین پیشنهادهای تامین‌کنندگان داشبورد",

            ["AttributeDefinitions.ActivateValue"] = "فعال‌سازی مقدار ویژگی فنی",
            ["AttributeDefinitions.CreateValue"] = "افزودن مقدار ویژگی فنی",
            ["AttributeDefinitions.DeactivateValue"] = "غیرفعال‌سازی مقدار ویژگی فنی",
            ["AttributeDefinitions.GetValues"] = "مشاهده مقادیر ویژگی فنی",
            ["AttributeDefinitions.UpdateValue"] = "ویرایش مقدار ویژگی فنی",
            ["CatalogItems.CommitImport"] = "ثبت نهایی ورود گروهی اقلام کاتالوگ",
            ["CatalogItems.GetAttributeOptions"] = "مشاهده گزینه‌های ویژگی اقلام کاتالوگ",
            ["CatalogItems.ValidateImport"] = "اعتبارسنجی فایل ورود اقلام کاتالوگ",
            ["Classifications.CommitImport"] = "ثبت نهایی ورود گروهی طبقه‌بندی‌ها",
            ["Classifications.GetAttributes"] = "مشاهده ویژگی‌های طبقه‌بندی",
            ["Classifications.GetChildren"] = "مشاهده زیرشاخه‌های طبقه‌بندی",
            ["Classifications.GetTree"] = "مشاهده درخت طبقه‌بندی‌ها",
            ["Classifications.SetAttributes"] = "تنظیم ویژگی‌های طبقه‌بندی",
            ["Classifications.ValidateImport"] = "اعتبارسنجی فایل ورود طبقه‌بندی‌ها",
            ["MasterDataRequests.GetMine"] = "مشاهده درخواست‌های داده پایه من",
            ["MasterDataRequests.GetPendingCounts"] = "مشاهده تعداد درخواست‌های داده پایه در انتظار بررسی",

            ["MyAdminAccesses.ChangeMyPassword"] = "تغییر گذرواژه حساب مدیریتی من",
            ["MyAdminAccesses.GetMyAccessVersion"] = "مشاهده نسخه دسترسی‌های حساب مدیریتی من",
            ["MyAdminAccesses.GetMyAccesses"] = "مشاهده دسترسی‌های حساب مدیریتی من",
            ["MyAdminAccesses.GetMyProfile"] = "مشاهده پروفایل حساب مدیریتی من",
            ["MyAdminAccesses.UpdateMyProfile"] = "ویرایش پروفایل حساب مدیریتی من",
            ["Feature.ReplaceAccesses"] = "تنظیم منابع دسترسی قابلیت",
            ["Feature.GetAccesses"] = "مشاهده منابع دسترسی قابلیت",
            ["RoleFeature.AddFeature"] = "اعطای قابلیت به نقش",
            ["RoleFeature.AddFeatureTree"] = "اعطای درخت قابلیت به نقش",
            ["RoleFeature.GetAllFeaturesWithSelection"] = "مشاهده همه قابلیت‌ها و انتخاب‌های نقش",
            ["RoleFeature.GetRoleFeatures"] = "مشاهده قابلیت‌های نقش",
            ["RoleFeature.RemoveFeature"] = "لغو قابلیت نقش",
            ["RoleFeature.RemoveFeatureTree"] = "لغو درخت قابلیت نقش",
            ["Roles.AddUserToRole"] = "افزودن کاربر به نقش",
            ["Roles.GetRoleUsers"] = "مشاهده کاربران نقش",
            ["Roles.GetUserRoles"] = "مشاهده نقش‌های کاربر",
            ["Roles.RemoveUserFromRole"] = "حذف کاربر از نقش",
            ["Users.AddRoleToUser"] = "افزودن نقش به کاربر",
            ["Users.ChangePassword"] = "تغییر گذرواژه کاربر",
            ["Users.GetManagementList"] = "مشاهده فهرست مدیریتی کاربران",
            ["Users.GetUserRoles"] = "مشاهده نقش‌های کاربر",
            ["Users.RemoveRoleFromUser"] = "حذف نقش از کاربر",
            ["Users.RequestPasswordReset"] = "درخواست بازنشانی گذرواژه",
            ["Vendors.RequestCorrection"] = "درخواست اصلاح پرونده تامین‌کننده",

            ["Notifications.GetAllForAdmin"] = "مشاهده همه اعلان‌ها برای مدیر",
            ["Notifications.GetEventCodes"] = "مشاهده کدهای رویداد اعلان‌ها",
            ["Notifications.GetMine"] = "مشاهده اعلان‌های من",
            ["Notifications.GetMyMessages"] = "مشاهده پیام‌های همه کانال‌های من",
            ["Notifications.GetPendingCount"] = "مشاهده تعداد اعلان‌های در انتظار",
            ["Notifications.GetUnreadCount"] = "مشاهده تعداد اعلان‌های خوانده‌نشده",
            ["Notifications.MarkAllAsRead"] = "علامت‌گذاری همه اعلان‌ها به‌عنوان خوانده‌شده",
            ["Notifications.MarkAsRead"] = "علامت‌گذاری اعلان به‌عنوان خوانده‌شده",
            ["SentMessages.GetEmail"] = "مشاهده ایمیل‌های ارسال‌شده",
            ["SentMessages.GetSms"] = "مشاهده پیامک‌های ارسال‌شده",
            ["Settings.GetHistory"] = "مشاهده تاریخچه تنظیمات",
            ["Settings.GetSettings"] = "مشاهده فهرست تنظیمات",
            ["TestManagement.GetHistory"] = "مشاهده تاریخچه اجرای آزمون",
            ["TestManagement.GetList"] = "مشاهده فهرست آزمون‌ها",
            ["TestManagement.GetTodayStats"] = "مشاهده آمار امروز آزمون‌ها",
            ["TestManagement.RecordExecution"] = "ثبت نتیجه اجرای آزمون",

            ["BuyerProfile.GetMessagesForAdmin"] = "مشاهده پیام‌های خریدار برای مدیر",
            ["BuyerProfile.GetMyOrganizationUsers"] = "مشاهده کاربران سازمان من",
            ["BuyerProfile.GetMyProfile"] = "مشاهده پروفایل خریدار من",
            ["BuyerProfile.GetUsersForAdmin"] = "مشاهده کاربران خریدار برای مدیر",
            ["BuyerProfile.RequestRevision"] = "درخواست اصلاح پروفایل خریدار",
            ["BuyerCeo.GetByBuyerProfileId"] = "مشاهده مدیرعامل خریدار",
            ["BuyerCeo.GetMine"] = "مشاهده اطلاعات مدیرعامل من",
            ["BuyerRequestQuota.GetByBuyerUserId"] = "مشاهده سهمیه درخواست خریدار",
            ["BuyerRequestQuota.GetMyQuota"] = "مشاهده سهمیه درخواست من",
            ["BuyerRequestQuota.SetQuota"] = "تنظیم سهمیه درخواست خریدار",
            ["EquipmentPriceCalculation.Calculate"] = "محاسبه قیمت تجهیزات",
            ["EquipmentPriceCalculation.GetMine"] = "مشاهده محاسبات قیمت تجهیزات من",

            ["PriceEstimation.Create"] = "ثبت برآورد اولیه قیمت",
            ["PriceEstimation.Finalize"] = "نهایی‌سازی برآورد قیمت درخواست",
            ["PriceEstimation.GetForRequest"] = "مشاهده برآورد قیمت درخواست",
            ["PriceEstimation.GetMine"] = "مشاهده برآورد قیمت درخواست من",
            ["PriceEstimation.Revise"] = "اصلاح برآورد قیمت درخواست",

            ["PurchaseRequest.ApproveProposal"] = "تایید پیشنهاد تامین‌کننده",
            ["PurchaseRequest.CloseProposalCollection"] = "بستن مهلت دریافت پیشنهادها",
            ["PurchaseRequest.CreateOrGetProposalDraft"] = "ایجاد یا بازیابی پیش‌نویس پیشنهاد",
            ["PurchaseRequest.DeclinePublishedInvitation"] = "رد دعوت درخواست خرید",
            ["PurchaseRequest.DownloadAttachment"] = "دریافت پیوست درخواست خرید",
            ["PurchaseRequest.DownloadAttachmentForAdmin"] = "دریافت پیوست درخواست خرید برای مدیر",
            ["PurchaseRequest.DownloadProposalAttachmentForAdmin"] = "دریافت پیوست پیشنهاد برای مدیر",
            ["PurchaseRequest.DownloadProposalAttachmentForBuyer"] = "دریافت پیوست پیشنهاد برای خریدار",
            ["PurchaseRequest.DownloadProposalProforma"] = "دریافت پیش‌فاکتور پیشنهاد",
            ["PurchaseRequest.DownloadProposalProformaForAdmin"] = "دریافت پیش‌فاکتور پیشنهاد برای مدیر",
            ["PurchaseRequest.DownloadPublishedAttachment"] = "دریافت پیوست درخواست خرید منتشرشده",
            ["PurchaseRequest.DownloadTechnicalProposal"] = "دریافت پیشنهاد فنی",
            ["PurchaseRequest.FinalizePrices"] = "نهایی‌سازی قیمت‌های درخواست خرید",
            ["PurchaseRequest.GetAdminSummary"] = "مشاهده خلاصه مدیریتی درخواست‌های خرید",
            ["PurchaseRequest.GetByIdForAdmin"] = "مشاهده جزئیات درخواست خرید برای مدیر",
            ["PurchaseRequest.GetHistory"] = "مشاهده تاریخچه درخواست خرید",
            ["PurchaseRequest.GetHistoryForAdmin"] = "مشاهده تاریخچه درخواست خرید برای مدیر",
            ["PurchaseRequest.GetInvitationRosterForAdmin"] = "مشاهده فهرست دعوت‌شدگان درخواست خرید",
            ["PurchaseRequest.GetMine"] = "مشاهده درخواست‌های خرید من",
            ["PurchaseRequest.GetPriceCalculations"] = "مشاهده محاسبات قیمت درخواست خرید",
            ["PurchaseRequest.GetProposalDetailForAdmin"] = "مشاهده جزئیات پیشنهاد برای مدیر",
            ["PurchaseRequest.GetProposalItems"] = "مشاهده اقلام پیشنهاد",
            ["PurchaseRequest.GetProposalProforma"] = "مشاهده پیش‌فاکتور پیشنهاد",
            ["PurchaseRequest.GetProposalReviewDetail"] = "مشاهده جزئیات بررسی پیشنهاد",
            ["PurchaseRequest.GetProposalSummaryForAdmin"] = "مشاهده خلاصه پیشنهادها برای مدیر",
            ["PurchaseRequest.GetProposalsForReview"] = "مشاهده پیشنهادهای آماده بررسی",
            ["PurchaseRequest.GetPublicationsForAdmin"] = "مشاهده سوابق انتشار درخواست خرید",
            ["PurchaseRequest.GetPublishedToMe"] = "مشاهده درخواست‌های خرید منتشرشده برای من",
            ["PurchaseRequest.GetPublishedToMeById"] = "مشاهده جزئیات درخواست خرید منتشرشده برای من",
            ["PurchaseRequest.GetTechnicalProposal"] = "مشاهده پیشنهاد فنی",
            ["PurchaseRequest.RejectProposal"] = "رد پیشنهاد تامین‌کننده",
            ["PurchaseRequest.RemoveProposalProforma"] = "حذف پیش‌فاکتور پیشنهاد",
            ["PurchaseRequest.RemoveTechnicalProposal"] = "حذف پیشنهاد فنی",
            ["PurchaseRequest.RequestRevision"] = "درخواست اصلاح درخواست خرید",
            ["PurchaseRequest.SaveProposalItems"] = "ذخیره اقلام پیشنهاد",
            ["PurchaseRequest.SearchSupplierCandidates"] = "جستجوی تامین‌کنندگان مناسب درخواست خرید",
            ["PurchaseRequest.SubmitProposal"] = "ارسال پیشنهاد تامین‌کننده",
            ["PurchaseRequest.UploadProposalProforma"] = "بارگذاری پیش‌فاکتور پیشنهاد",
            ["PurchaseRequest.UploadTechnicalProposal"] = "بارگذاری پیشنهاد فنی",
            ["QuotaChargeRequest.GetMine"] = "مشاهده درخواست‌های شارژ سهمیه من",
            ["QuotaChargeRequest.GetMyTimeline"] = "مشاهده گردش سهمیه من",
            ["QuotaChargeRequest.GetTimelineForAdmin"] = "مشاهده گردش سهمیه خریدار برای مدیر",
            ["QuotaChargeRequest.GetUnitPrice"] = "مشاهده قیمت واحد شارژ سهمیه",

            ["VendorActivityDeclaration.GetMine"] = "مشاهده اظهار فعالیت من",
            ["VendorActivityReview.Review"] = "بررسی اظهار فعالیت تامین‌کننده",
            ["VendorAdminWork.GetPendingBySupplier"] = "مشاهده کارهای مدیریتی در انتظار به تفکیک تامین‌کننده"
        };

    public static IReadOnlyList<EndpointAccessDefinitionDto> Build(
        IApiDescriptionGroupCollectionProvider apiDescriptions)
    {
        var candidates = apiDescriptions.ApiDescriptionGroups.Items
            .SelectMany(group => group.Items)
            .Select(CreateCandidate)
            .Where(candidate => candidate is not null)
            .Cast<Candidate>()
            .ToList();

        var definitions = new List<EndpointAccessDefinitionDto>(candidates.Count);
        foreach (var group in candidates.GroupBy(
                     item => $"{item.Module}.{item.Controller}.{item.Action}",
                     StringComparer.OrdinalIgnoreCase))
        {
            foreach (var candidate in group)
            {
                definitions.Add(new EndpointAccessDefinitionDto(
                    EndpointAccessIdentity.Application,
                    candidate.Module,
                    candidate.Controller,
                    candidate.Action,
                    candidate.HttpMethod,
                    candidate.RoutePattern,
                    ResolveKey(candidate, group.Count() > 1),
                    BuildPersianTitle(candidate),
                    (byte)candidate.AccessMode,
                    candidate.FeatureKeys,
                    ResolveModuleTitle(candidate.Module),
                    ResolveControllerTitle(candidate.Controller),
                    (byte)candidate.Authorization.Panel,
                    (byte)candidate.Authorization.Requirements));
            }
        }

        var duplicateIdentity = definitions
            .GroupBy(item => $"{item.Application}\n{item.HttpMethod}\n{item.RoutePattern}", StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault(group => group.Count() > 1);
        if (duplicateIdentity is not null)
        {
            throw new InvalidOperationException($"Duplicate endpoint identity '{duplicateIdentity.Key}'.");
        }

        return definitions
            .OrderBy(item => item.Module, StringComparer.Ordinal)
            .ThenBy(item => item.Controller, StringComparer.Ordinal)
            .ThenBy(item => item.Action, StringComparer.Ordinal)
            .ThenBy(item => item.HttpMethod, StringComparer.Ordinal)
            .ThenBy(item => item.RoutePattern, StringComparer.Ordinal)
            .ToList();
    }

    private static Candidate? CreateCandidate(ApiDescription description)
    {
        if (description.ActionDescriptor is not ControllerActionDescriptor action
            || string.IsNullOrWhiteSpace(description.HttpMethod)
            || string.IsNullOrWhiteSpace(description.RelativePath))
        {
            return null;
        }

        var module = ResolveModule(action.ControllerTypeInfo.Assembly.GetName().Name);
        var method = EndpointAccessIdentity.NormalizeMethod(description.HttpMethod);
        var route = EndpointAccessIdentity.NormalizeRoute(description.RelativePath);
        var rule = AdminAccessRuleCatalog.Find(action.ControllerName, action.ActionName);
        // TASK-0247: panel/requirements come from the explicit catalog, never from attributes; an
        // unclassified action throws here, so it fails the catalog tests and startup sync.
        var authorization = EndpointAuthorizationCatalog.Resolve(action.ControllerName, action.ActionName);

        var mode = authorization.Panel switch
        {
            AccessPanel.Admin => AccessMode.AdminFeatureGated,
            AccessPanel.Public => AccessMode.Public,
            _ when module == "catalogs" && method == "GET" => AccessMode.SharedReadAuthenticated,
            _ => AccessMode.SelfServiceAuthenticated
        };

        return new Candidate(
            module, action.ControllerName, action.ActionName, method, route, mode, rule?.Keys ?? [], authorization);
    }

    private static string ResolveKey(Candidate candidate, bool requiresVariant)
    {
        var identity = $"{candidate.HttpMethod} {candidate.RoutePattern}";
        if (ExplicitKeys.TryGetValue(identity, out var explicitKey))
        {
            return explicitKey;
        }

        if (requiresVariant)
        {
            throw new InvalidOperationException(
                $"Endpoint '{candidate.Controller}.{candidate.Action}' has multiple routes/methods and requires an explicit stable key for '{identity}'.");
        }

        return EndpointAccessIdentity.NormalizeKey($"{candidate.Module}.{candidate.Controller}.{candidate.Action}");
    }

    private static string ResolveModule(string? assemblyName)
    {
        if (string.IsNullOrWhiteSpace(assemblyName))
        {
            throw new InvalidOperationException("Controller assembly has no name.");
        }

        var parts = assemblyName.Split('.', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 2 || !string.Equals(parts[0], "PM", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"Cannot resolve product module from '{assemblyName}'.");
        }

        return parts[1].ToLowerInvariant();
    }

    /// <summary>TASK-0181: Persian title for the Access resource tree's module-grouping node.</summary>
    private static string ResolveModuleTitle(string module)
    {
        if (!ModuleTitles.TryGetValue(module, out var title))
        {
            throw new InvalidOperationException($"Module '{module}' needs an approved Persian title.");
        }

        return title;
    }

    /// <summary>TASK-0181: Persian title for the Access resource tree's controller-grouping node.</summary>
    private static string ResolveControllerTitle(string controller)
    {
        if (!ResourceTitles.TryGetValue(controller, out var title))
        {
            throw new InvalidOperationException($"Controller '{controller}' needs an approved Persian resource title.");
        }

        return title;
    }

    private static string BuildPersianTitle(Candidate candidate)
    {
        var routeIdentity = $"{candidate.HttpMethod} {candidate.RoutePattern}";
        if (ExplicitPersianTitles.TryGetValue(routeIdentity, out var explicitRouteTitle))
        {
            return explicitRouteTitle;
        }

        var actionIdentity = $"{candidate.Controller}.{candidate.Action}";
        if (ExplicitPersianTitles.TryGetValue(actionIdentity, out var explicitActionTitle))
        {
            return explicitActionTitle;
        }

        var action = candidate.Action;
        var controller = candidate.Controller;
        if (!ResourceTitles.TryGetValue(controller, out var resource))
        {
            throw new InvalidOperationException($"Controller '{controller}' needs an approved Persian resource title.");
        }

        var operation = action switch
        {
            "Global" => "مشاهده نسخه عمومی",
            "Develop" => "مشاهده نسخه توسعه",
            "AdminResetPassword" => "بازنشانی مدیریتی گذرواژه کاربران",
            "GetList" => "مشاهده فهرست",
            "GetById" => "مشاهده جزئیات",
            var value when value.StartsWith("Download", StringComparison.Ordinal) => "دریافت فایل",
            var value when value.StartsWith("Upload", StringComparison.Ordinal) => "بارگذاری فایل",
            var value when value.StartsWith("Get", StringComparison.Ordinal) => "مشاهده",
            var value when value.StartsWith("Search", StringComparison.Ordinal) => "جستجو در",
            var value when value.StartsWith("Create", StringComparison.Ordinal) => "ایجاد",
            var value when value.StartsWith("Register", StringComparison.Ordinal) => "ثبت",
            var value when value.StartsWith("Submit", StringComparison.Ordinal) => "ارسال",
            var value when value.StartsWith("Save", StringComparison.Ordinal) => "ذخیره",
            var value when value.StartsWith("Set", StringComparison.Ordinal) => "تنظیم",
            var value when value.StartsWith("Update", StringComparison.Ordinal) => "ویرایش",
            var value when value.StartsWith("Edit", StringComparison.Ordinal) => "ویرایش",
            var value when value.StartsWith("Revise", StringComparison.Ordinal) => "اصلاح",
            var value when value.StartsWith("Delete", StringComparison.Ordinal) => "حذف",
            var value when value.StartsWith("Remove", StringComparison.Ordinal) => "حذف",
            var value when value.StartsWith("Approve", StringComparison.Ordinal) => "تایید",
            var value when value.StartsWith("Reject", StringComparison.Ordinal) => "رد",
            var value when value.StartsWith("Decline", StringComparison.Ordinal) => "رد",
            var value when value.StartsWith("Activate", StringComparison.Ordinal) => "فعال‌سازی",
            var value when value.StartsWith("Deactivate", StringComparison.Ordinal) => "غیرفعال‌سازی",
            var value when value.StartsWith("Cancel", StringComparison.Ordinal) => "لغو",
            var value when value.StartsWith("Publish", StringComparison.Ordinal) => "انتشار",
            var value when value.StartsWith("Validate", StringComparison.Ordinal) => "اعتبارسنجی",
            var value when value.StartsWith("Calculate", StringComparison.Ordinal) => "محاسبه",
            var value when value.StartsWith("Add", StringComparison.Ordinal) => "افزودن",
            var value when value.StartsWith("Mark", StringComparison.Ordinal) => "علامت‌گذاری",
            var value when value.StartsWith("Change", StringComparison.Ordinal) => "تغییر",
            var value when value.StartsWith("Reset", StringComparison.Ordinal) => "بازنشانی",
            var value when value.StartsWith("Request", StringComparison.Ordinal) => "درخواست",
            var value when value.StartsWith("Execute", StringComparison.Ordinal) => "اجرای",
            var value when value.StartsWith("Record", StringComparison.Ordinal) => "ثبت اجرای",
            var value when value.StartsWith("Review", StringComparison.Ordinal) => "بررسی",
            var value when value.StartsWith("Commit", StringComparison.Ordinal) => "ثبت نهایی",
            var value when value.StartsWith("Close", StringComparison.Ordinal) => "بستن",
            var value when value.StartsWith("Finalize", StringComparison.Ordinal) => "نهایی‌سازی",
            var value when value.StartsWith("Extend", StringComparison.Ordinal) => "تمدید",
            var value when value.StartsWith("Refresh", StringComparison.Ordinal) => "تازه‌سازی",
            var value when value.StartsWith("Logout", StringComparison.Ordinal) => "خروج از",
            var value when value.StartsWith("Login", StringComparison.Ordinal) => "ورود به",
            _ => throw new InvalidOperationException(
                $"Action '{controller}.{action}' needs an approved Persian operation title.")
        };

        return action is "Global" or "Develop" or "AdminResetPassword"
            ? operation
            : $"{operation} {resource}";
    }

    private sealed record Candidate(
        string Module,
        string Controller,
        string Action,
        string HttpMethod,
        string RoutePattern,
        AccessMode AccessMode,
        IReadOnlyCollection<string> FeatureKeys,
        EndpointAuthorization Authorization);
}
