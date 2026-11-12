using PM.Identities.Core.Contract.Accesses;
using PM.Identities.Core.Domain.Features;
using PM.Identities.Infra.DAL.DbContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace PM.Identities.Core.ApplicationService.Features;

/// <summary>
/// Seeds the static Admin panel Feature catalog (idempotent upsert by Key).
/// </summary>
public static class AdminFeatureCatalogSeeder
{
    public static async Task<AdminFeatureCatalogSeedResult> SeedAsync(
        PMIdentityDbContext dbContext,
        ILogger? logger = null)
    {
        var nodes = BuildNodes();
        var managedKeys = nodes
            .Select(node => AccessKeyNormalizer.Normalize(node.Key))
            .ToHashSet(StringComparer.Ordinal);
        var existing = await dbContext.Features.ToListAsync();
        var byKey = existing.ToDictionary(
            feature => AccessKeyNormalizer.Normalize(feature.Key),
            feature => feature,
            StringComparer.Ordinal);
        var inserted = 0;
        var insertedKeys = new HashSet<string>(StringComparer.Ordinal);
        var changedKeys = new HashSet<string>(StringComparer.Ordinal);

        foreach (var node in nodes)
        {
            var norm = AccessKeyNormalizer.Normalize(node.Key);
            if (byKey.TryGetValue(norm, out var feature))
            {
                var changed = feature.PersianTitle != node.PersianTitle
                    || !feature.Enable
                    || feature.Dependent
                    || !string.Equals(feature.Key, node.Key, StringComparison.Ordinal);
                feature.PersianTitle = node.PersianTitle;
                feature.Enable = true;
                feature.Dependent = false;
                if (!string.Equals(feature.Key, node.Key, StringComparison.Ordinal))
                {
                    feature.Key = node.Key;
                }

                if (changed)
                {
                    changedKeys.Add(norm);
                }
            }
            else
            {
                feature = new Feature
                {
                    Key = node.Key,
                    PersianTitle = node.PersianTitle,
                    Enable = true,
                    Dependent = false
                };
                dbContext.Features.Add(feature);
                byKey[norm] = feature;
                inserted++;
                insertedKeys.Add(norm);
            }
        }

        await dbContext.SaveChangesAsync();

        existing = await dbContext.Features.ToListAsync();
        byKey = existing.ToDictionary(
            a => AccessKeyNormalizer.Normalize(a.Key),
            a => a,
            StringComparer.Ordinal);

        foreach (var node in nodes)
        {
            var feature = byKey[AccessKeyNormalizer.Normalize(node.Key)];
            long? parentId = null;
            if (!string.IsNullOrEmpty(node.ParentKey))
            {
                parentId = byKey[AccessKeyNormalizer.Normalize(node.ParentKey)].Id;
            }

            if (feature.ParentId != parentId)
            {
                feature.ParentId = parentId;
                var normalizedKey = AccessKeyNormalizer.Normalize(node.Key);
                if (!insertedKeys.Contains(normalizedKey))
                {
                    changedKeys.Add(normalizedKey);
                }
            }
        }

        await dbContext.SaveChangesAsync();
        var unmanagedFeatureIds = existing
            .Where(feature => !managedKeys.Contains(AccessKeyNormalizer.Normalize(feature.Key)))
            .Select(feature => feature.Id)
            .ToArray();
        var unmanagedRoleGrants = unmanagedFeatureIds.Length == 0
            ? 0
            : await dbContext.RoleFeatures.CountAsync(link => unmanagedFeatureIds.Contains(link.FeatureId));
        var result = new AdminFeatureCatalogSeedResult(
            nodes.Count,
            inserted,
            changedKeys.Count,
            unmanagedFeatureIds.Length,
            unmanagedRoleGrants);

        logger?.LogInformation(
            "Admin Feature catalog reconciled: {Managed} managed, {Inserted} inserted, {Updated} updated, " +
            "{Unmanaged} unmanaged preserved, {UnmanagedRoleGrants} unmanaged role grants preserved.",
            result.ManagedDefinitions,
            result.Inserted,
            result.Updated,
            result.UnmanagedPreserved,
            result.UnmanagedRoleGrantsPreserved);
        return result;
    }

    private static List<AdminFeatureCatalogNodeDef> BuildNodes() =>
    [
        new(AdminAccessKeys.Root, "پنل ادمین", null),

        new("Admin.Dashboard", "داشبورد", AdminAccessKeys.Root),
        new(AdminAccessDashboardKeys.View, "مشاهده داشبورد", "Admin.Dashboard"),

        new("Admin.Guide", "راهنما", AdminAccessKeys.Root),
        new(AdminAccessGuideKeys.View, "مشاهده راهنما", "Admin.Guide"),

        new(AdminAccessUsersKeys.Node, "کاربران", AdminAccessKeys.Root),
        new(AdminAccessUsersKeys.View, "مشاهده کاربران", AdminAccessUsersKeys.Node),
        new(AdminAccessUsersKeys.Create, "ایجاد کاربر", AdminAccessUsersKeys.Node),
        new(AdminAccessUsersKeys.Update, "ویرایش کاربر", AdminAccessUsersKeys.Node),
        new(AdminAccessUsersKeys.Activate, "فعال‌سازی کاربر", AdminAccessUsersKeys.Node),
        new(AdminAccessUsersKeys.Deactivate, "غیرفعال‌سازی کاربر", AdminAccessUsersKeys.Node),
        new(AdminAccessUsersKeys.ManageRoles, "مدیریت نقش‌های کاربر", AdminAccessUsersKeys.Node),
        new(AdminAccessUsersKeys.ResetPassword, "بازنشانی رمز عبور", AdminAccessUsersKeys.Node),

        new(AdminAccessRolesKeys.Node, "نقش‌ها", AdminAccessKeys.Root),
        new(AdminAccessRolesKeys.View, "مشاهده نقش‌ها", AdminAccessRolesKeys.Node),
        new(AdminAccessRolesKeys.Create, "ایجاد نقش", AdminAccessRolesKeys.Node),
        new(AdminAccessRolesKeys.Update, "ویرایش نقش", AdminAccessRolesKeys.Node),
        new(AdminAccessRolesKeys.Activate, "فعال‌سازی نقش", AdminAccessRolesKeys.Node),
        new(AdminAccessRolesKeys.Deactivate, "غیرفعال‌سازی نقش", AdminAccessRolesKeys.Node),
        new(AdminAccessRolesKeys.ManageUsers, "مدیریت کاربران نقش", AdminAccessRolesKeys.Node),
        new(AdminAccessRolesKeys.ManageAccesses, "مدیریت دسترسی‌های نقش", AdminAccessRolesKeys.Node),

        new(AdminAccessPermissionsKeys.Node, "دسترسی‌ها", AdminAccessKeys.Root),
        new(AdminAccessPermissionsKeys.View, "مشاهده دسترسی‌ها", AdminAccessPermissionsKeys.Node),
        new(AdminAccessPermissionsKeys.Create, "ایجاد دسترسی", AdminAccessPermissionsKeys.Node),
        new(AdminAccessPermissionsKeys.Update, "ویرایش دسترسی", AdminAccessPermissionsKeys.Node),
        new(AdminAccessPermissionsKeys.Delete, "حذف دسترسی", AdminAccessPermissionsKeys.Node),

        new(AdminAccessQcKeys.Node, "کیفیت و تست", AdminAccessKeys.Root),
        new(AdminAccessQcKeys.DashboardView, "مشاهده داشبورد تست", AdminAccessQcKeys.Node),
        new(AdminAccessQcKeys.RecordsView, "مشاهده تست‌ها", AdminAccessQcKeys.Node),
        new(AdminAccessQcKeys.RecordsCreate, "ایجاد تست", AdminAccessQcKeys.Node),
        new(AdminAccessQcKeys.RecordsUpdate, "ویرایش تست", AdminAccessQcKeys.Node),
        new(AdminAccessQcKeys.RecordsDelete, "حذف تست", AdminAccessQcKeys.Node),

        new(AdminAccessOrganizationsKeys.Node, "سازمان‌ها", AdminAccessKeys.Root),
        new(AdminAccessOrganizationsKeys.CatalogCreate, "ایجاد سازمان", AdminAccessOrganizationsKeys.Node),

        new(AdminAccessBuyersKeys.Node, "خریداران", AdminAccessKeys.Root),
        new(AdminAccessBuyersKeys.View, "مشاهده خریداران", AdminAccessBuyersKeys.Node),
        new(AdminAccessBuyersKeys.Activate, "فعال‌سازی خریدار", AdminAccessBuyersKeys.Node),
        new(AdminAccessBuyersKeys.Deactivate, "غیرفعال‌سازی خریدار", AdminAccessBuyersKeys.Node),

        new(AdminAccessVendorsKeys.Node, "تأمین‌کنندگان", AdminAccessKeys.Root),
        new(AdminAccessVendorsKeys.View, "مشاهده تأمین‌کنندگان", AdminAccessVendorsKeys.Node),
        new(AdminAccessVendorsKeys.Review, "بررسی تأمین‌کننده", AdminAccessVendorsKeys.Node),
        new(AdminAccessVendorsKeys.Activate, "فعال‌سازی تأمین‌کننده", AdminAccessVendorsKeys.Node),
        new(AdminAccessVendorsKeys.Deactivate, "غیرفعال‌سازی تأمین‌کننده", AdminAccessVendorsKeys.Node),

        new("Admin.MasterData", "داده‌های پایه", AdminAccessKeys.Root),
        new("Admin.MasterData.UnitMeasures", "واحدهای اندازه‌گیری", "Admin.MasterData"),
        new("Admin.MasterData.UnitMeasures.View", "مشاهده واحدها", "Admin.MasterData.UnitMeasures"),
        new("Admin.MasterData.UnitMeasures.Create", "ایجاد واحد", "Admin.MasterData.UnitMeasures"),
        new("Admin.MasterData.UnitMeasures.Update", "ویرایش واحد", "Admin.MasterData.UnitMeasures"),
        new("Admin.MasterData.UnitMeasures.Delete", "حذف واحد", "Admin.MasterData.UnitMeasures"),
        new("Admin.MasterData.Countries", "کشورها", "Admin.MasterData"),
        new("Admin.MasterData.Countries.View", "مشاهده کشورها", "Admin.MasterData.Countries"),
        new("Admin.MasterData.Countries.Create", "ایجاد کشور", "Admin.MasterData.Countries"),
        new("Admin.MasterData.Countries.Update", "ویرایش کشور", "Admin.MasterData.Countries"),
        new("Admin.MasterData.Countries.Delete", "حذف کشور", "Admin.MasterData.Countries"),
        new("Admin.MasterData.Brands", "برندها", "Admin.MasterData"),
        new("Admin.MasterData.Brands.View", "مشاهده برندها", "Admin.MasterData.Brands"),
        new("Admin.MasterData.Brands.Create", "ایجاد برند", "Admin.MasterData.Brands"),
        new("Admin.MasterData.Brands.Update", "ویرایش برند", "Admin.MasterData.Brands"),
        new("Admin.MasterData.Brands.Delete", "حذف برند", "Admin.MasterData.Brands"),
        new("Admin.MasterData.Classifications", "طبقه‌بندی سلسله‌مراتبی", "Admin.MasterData"),
        new("Admin.MasterData.Classifications.View", "مشاهده طبقه‌بندی سلسله‌مراتبی", "Admin.MasterData.Classifications"),
        new("Admin.MasterData.Classifications.Create", "ایجاد طبقه‌بندی", "Admin.MasterData.Classifications"),
        new("Admin.MasterData.Classifications.Update", "ویرایش طبقه‌بندی", "Admin.MasterData.Classifications"),
        new("Admin.MasterData.Classifications.Delete", "حذف طبقه‌بندی", "Admin.MasterData.Classifications"),
        new("Admin.MasterData.CatalogItems", "اقلام کاتالوگ", "Admin.MasterData"),
        new("Admin.MasterData.CatalogItems.View", "مشاهده اقلام", "Admin.MasterData.CatalogItems"),
        new("Admin.MasterData.CatalogItems.Create", "ایجاد قلم", "Admin.MasterData.CatalogItems"),
        new("Admin.MasterData.CatalogItems.Update", "ویرایش قلم", "Admin.MasterData.CatalogItems"),
        new("Admin.MasterData.CatalogItems.Delete", "حذف قلم", "Admin.MasterData.CatalogItems"),
        new("Admin.MasterData.ProductAttributes", "ویژگی‌های کالا", "Admin.MasterData"),
        new("Admin.MasterData.ProductAttributes.View", "مشاهده ویژگی‌های کالا", "Admin.MasterData.ProductAttributes"),
        new("Admin.MasterData.ProductAttributes.Create", "ایجاد ویژگی کالا", "Admin.MasterData.ProductAttributes"),
        new("Admin.MasterData.ProductAttributes.Update", "ویرایش ویژگی کالا", "Admin.MasterData.ProductAttributes"),

        new("Admin.Equipment", "تجهیزات", AdminAccessKeys.Root),
        new("Admin.Equipment.View", "مشاهده تجهیزات", "Admin.Equipment"),
        new("Admin.Equipment.Create", "ایجاد تجهیز", "Admin.Equipment"),
        new("Admin.Equipment.Update", "ویرایش تجهیز", "Admin.Equipment"),
        new("Admin.Equipment.Delete", "حذف تجهیز", "Admin.Equipment"),

        new("Admin.MasterDataRequests", "درخواست‌های داده پایه", AdminAccessKeys.Root),
        new("Admin.MasterDataRequests.View", "مشاهده درخواست‌های داده پایه", "Admin.MasterDataRequests"),
        new("Admin.MasterDataRequests.Decide", "تصمیم‌گیری درخواست‌های داده پایه", "Admin.MasterDataRequests"),

        new("Admin.PurchaseRequests", "درخواست‌های خرید", AdminAccessKeys.Root),
        new("Admin.PurchaseRequests.View", "مشاهده درخواست‌های خرید", "Admin.PurchaseRequests"),
        new("Admin.PurchaseRequests.Decide", "تصمیم‌گیری درخواست‌های خرید", "Admin.PurchaseRequests"),
        new("Admin.PurchaseRequests.Publish", "انتشار درخواست‌های خرید", "Admin.PurchaseRequests"),

        new("Admin.Capacity", "ظرفیت خرید", AdminAccessKeys.Root),
        new("Admin.Capacity.Quota", "سقف درخواست خرید", "Admin.Capacity"),
        new("Admin.Capacity.Quota.View", "مشاهده سقف درخواست", "Admin.Capacity.Quota"),
        new("Admin.Capacity.Quota.Manage", "مدیریت سقف درخواست", "Admin.Capacity.Quota"),
        new("Admin.Capacity.ChargeRequests", "درخواست‌های افزایش سقف", "Admin.Capacity"),
        new("Admin.Capacity.ChargeRequests.View", "مشاهده درخواست‌های افزایش سقف", "Admin.Capacity.ChargeRequests"),
        new("Admin.Capacity.ChargeRequests.Decide", "تصمیم‌گیری درخواست‌های افزایش سقف", "Admin.Capacity.ChargeRequests"),

        new("Admin.VendorDeclarations.Activity", "اعلام فعالیت تأمین‌کننده", AdminAccessKeys.Root),
        new("Admin.VendorDeclarations.Activity.View", "مشاهده اعلام فعالیت", "Admin.VendorDeclarations.Activity"),
        new("Admin.VendorDeclarations.Activity.Decide", "تأیید/رد اعلام فعالیت", "Admin.VendorDeclarations.Activity"),

        new(AdminAccessNotificationsKeys.Node, "اعلان‌ها", AdminAccessKeys.Root),
        new(AdminAccessNotificationsKeys.View, "مشاهده اعلان‌ها", AdminAccessNotificationsKeys.Node),

        new(AdminAccessSentMessagesKeys.Node, "پیامک/ایمیل‌های ارسالی", AdminAccessKeys.Root),
        new(AdminAccessSentMessagesKeys.View, "مشاهده پیامک/ایمیل‌های ارسالی", AdminAccessSentMessagesKeys.Node),
    ];
}
