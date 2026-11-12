namespace PM.Identities.EndpointApi.Models;

/// <summary>
/// بدنهٔ درخواست ثبت‌نام تأمین‌کننده: اطلاعات هویتی (FR-AUTH-01/02) + نوع فروشنده (حقیقی/حقوقی،
/// BR-34 — افزوده‌شده 2026/09/04). اگر <see cref="IsLegalEntity"/> باشد، اطلاعات حقوقی شرکت
/// (FR-AUTH-05, ENT-03) هم الزامی است؛ در غیر این صورت فیلدهای شرکت نادیده گرفته می‌شوند.
/// برای شخص حقوقی، کد ملی یازده‌رقمی در <c>LegalNationalId</c> نام کاربری ورود نیز هست و
/// مقدار جداگانهٔ <c>UserName</c> توسط سرویس نادیده گرفته می‌شود.
/// بدون OTP/تأیید مدیر (به Task آینده موکول شد).
/// </summary>
public sealed record RegisterVendorRequestDto(
    string? UserName,
    string? Email,
    string? PhoneNumber,
    string Password,
    string? FirstName,
    string? LastName,
    bool IsLegalEntity,
    string? CompanyName,
    string? RegistrationNumber,
    string? EconomicCode,
    string? Address,
    string? CompanyPhoneNumber,
    string? NationalCode = null, string? LegalNationalId = null, string? ConfirmPassword = null);
