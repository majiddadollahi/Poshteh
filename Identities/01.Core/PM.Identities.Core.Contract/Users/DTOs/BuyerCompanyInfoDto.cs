namespace PM.Identities.Core.Contract.Users.DTOs;

/// <summary>
/// خلاصهٔ اطلاعات شرکت خریدار برای پاسخ لاگین — عمداً فیلدهای ساده (نه ارجاع به نوع Procurement)
/// تا این پروژه (Identities.Core.Contract) به ماژول دیگری وابسته نشود.
/// </summary>
public sealed class BuyerCompanyInfoDto
{
    public string CompanyName { get; set; } = string.Empty;
    public string LegalId { get; set; } = string.Empty;
    public string EconomicCode { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;

    /// <summary>وضعیت فعال/غیرفعال کسب‌وکاری خریدار (مستقل از تأیید ادمین) — برای محدودسازی Frontend بعد از ورود.</summary>
    public bool IsActive { get; set; }
    public string Status { get; set; } = "PendingApproval";
    public string? RevisionReason { get; set; }
}
