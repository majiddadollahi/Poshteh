using PM.Identities.Core.Contract.Users;
using PM.Identities.Core.Domain.Users;
using PM.Vendor.Core.Contract.Integration.Companies;
using Poshteh.Core.Domain;
namespace PM.Identities.Core.ApplicationService.Users;

public static class VendorProfileRules
{
    public static bool CanEdit(VendorReviewStatus status) =>
        status is VendorReviewStatus.ProfileIncomplete or VendorReviewStatus.Rejected or VendorReviewStatus.PendingCorrection;
    public static IReadOnlyList<string> Missing(User user, VendorBusinessProfile profile)
    {
        var missing = new List<string>();
        void Required(string? value, string label) { if (string.IsNullOrWhiteSpace(value)) missing.Add(label); }
        if (profile.Company is { } c)
        {
            Required(c.Name, "نام شرکت");
            if (!IranianIdentifiers.IsLegal(c.LegalNationalId)) missing.Add("شناسه ملی حقوقی معتبر");
            Required(c.RegistrationNumber, "شماره ثبت"); Required(c.EconomicCode, "کد اقتصادی");
            Required(c.Address, "آدرس شرکت"); Required(c.PhoneNumber, "تلفن شرکت");
        }
        else
        {
            Required(user.FirstName, "نام"); Required(user.LastName, "نام خانوادگی");
            if (!IranianIdentifiers.IsPersonal(user.NationalId)) missing.Add("کد ملی معتبر");
            if (!System.Text.RegularExpressions.Regex.IsMatch(IranianIdentifiers.Normalize(user.PhoneNumber), @"^(09\d{9}|\+989\d{9})$"))
                missing.Add("شماره موبایل معتبر");
            Required(profile.IndividualAddress, "آدرس");
        }
        return missing;
    }
    public static Error Forbidden => new("Vendor.Forbidden", "دسترسی به این عملیات مجاز نیست.");
    public static Error Conflict => new("Vendor.StatusConflict", "وضعیت پروفایل تغییر کرده یا اجازه این عملیات را نمی‌دهد.");
}
