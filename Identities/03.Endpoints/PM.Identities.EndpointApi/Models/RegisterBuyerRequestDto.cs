namespace PM.Identities.EndpointApi.Models;

/// <summary>
/// بدنهٔ درخواست ثبت‌نام خریدار: اطلاعات هویتی (FR-AUTH-01/02) + اطلاعات حقوقی شرکت خریدار
/// (FR-AUTH-04, ENT-18 — اصلاح 2026/08/27 پاسخ OQ-18). بدون OTP/تأیید مدیر (به Task آینده موکول شد).
/// </summary>
public sealed record RegisterBuyerRequestDto(
    string? UserName,
    string? Email,
    string PhoneNumber,
    string Password,
    string CompanyName,
    string LegalId,
    string? Address,
    string? CompanyPhoneNumber,
    string? ConfirmPassword = null);
