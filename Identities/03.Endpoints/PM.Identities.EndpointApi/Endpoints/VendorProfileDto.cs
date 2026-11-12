using PM.Identities.Core.Domain.Users;

namespace PM.Identities.EndpointApi.Endpoints;

// TASK-0020: یک وضعیت واحد (VendorReviewStatus) کل lifecycle کسب‌وکاری تأمین‌کننده را نشان می‌دهد.
// User.IsActive (قفل ورود Identity) و Company.IsActive (حذف‌شده) دیگر بخشی از این قرارداد نیستند؛
// activate/deactivate اکنون بر اساس همین Id (Vendor UserId) کار می‌کند، نه Company Id.
public sealed record VendorProfileDto(long Id, string VendorType, string Name, string ContactName,
    string? NationalId, string? PhoneNumber, string? Address, string? RegistrationNumber,
    string? EconomicCode, string? CompanyPhoneNumber, VendorReviewStatus Status,
    string? RejectionReason, string? CorrectionReason, DateTime? ReviewedAt,
    string UserName, string? Email, bool IsUserActive, DateTime? RegisteredAt, DateTime? LastLoginDate);
