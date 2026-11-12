namespace PM.Identities.Core.Contract.Users.DTOs;

/// <summary>
/// خلاصهٔ اطلاعات شرکت تأمین‌کننده برای پاسخ لاگین — عمداً فیلدهای ساده (نه ارجاع به نوع Vendor)
/// تا این پروژه (Identities.Core.Contract) به ماژول دیگری وابسته نشود.
/// </summary>
public sealed class SupplierCompanyInfoDto
{
    public string CompanyName { get; set; } = string.Empty;
    public string? RegistrationNumber { get; set; }
    public string? EconomicCode { get; set; }
    public string? Address { get; set; }
    public string? PhoneNumber { get; set; }
    public string? LegalNationalId { get; set; }
}
