namespace PM.Identities.Core.Contract.Integration.Users;

/// <summary>شمارش فعلی پروفایل‌های تأمین‌کنندهٔ منتظر بررسی ادمین (VendorReviewStatus = Pending).</summary>
public interface IIdentityAdminWorkReaderContract
{
    Task<int> CountPendingVendorProfilesAsync(CancellationToken cancellationToken = default);
}
