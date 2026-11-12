namespace PM.Identities.Core.Contract.Auth;

public static class MahtaJwtClaimTypes
{
    public const string UserId = "UserId";
    public const string TenantIds = "TenantIds";
    public const string SecurityStamp = "SecurityStamp";
    public const string AccountType = "AccountType";
    public const string DisplayName = "DisplayName";

    /// <summary>شناسهٔ BuyerProfile کاربر دارای رابطهٔ خریدار — صرفاً برای شناسایی موجودیت، نه تصمیم امنیتی.</summary>
    public const string BuyerId = "BuyerId";

    /// <summary>شناسهٔ Company حساب Vendor حقوقی — صرفاً برای شناسایی موجودیت، نه تصمیم امنیتی.</summary>
    public const string SupplierId = "SupplierId";

    /// <summary>
    /// عکس‌فوری IsActive خریدار/تأمین‌کننده در لحظهٔ صدور توکن — فقط برای UX سریع Frontend.
    /// Backend هرگز این Claim را برای تصمیم امنیتی معتبر نمی‌داند (همیشه از دیتابیس می‌خواند،
    /// چون ممکن است بعد از صدور توکن مدیر سامانه وضعیت را تغییر داده باشد).
    /// </summary>
    public const string IsActive = "IsActive";
}
