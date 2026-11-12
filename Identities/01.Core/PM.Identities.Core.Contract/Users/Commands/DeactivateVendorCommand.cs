using PM.Identities.Core.Contract.Users.DTOs;
using PM.Identities.Core.Domain.Users;
using Poshteh.Core.ApplicationService.Commands;
namespace PM.Identities.Core.Contract.Users.Commands;

/// <summary>
/// غیرفعال‌سازی/فعال‌سازی مجدد کسب‌وکاری تأمین‌کننده توسط مدیر سامانه (TASK-0020) — جانشین
/// Activate/DeactivateCompanyCommand قدیمی ماژول Vendor. برخلاف آن نسخه، برای تأمین‌کنندهٔ حقیقی
/// و حقوقی یکسان روی همین lifecycle واحد (<see cref="VendorReviewStatus"/>) کار می‌کند، نه فقط
/// شرکت‌های حقوقی. مثل ReviewVendorCommand، expected-status را چک می‌کند و در VendorReviewEntry
/// ثبت تاریخچه می‌شود.
/// </summary>
public sealed record DeactivateVendorCommand(long UserId, long AdminUserId) : ICommand<VendorOnboardingDto>;
