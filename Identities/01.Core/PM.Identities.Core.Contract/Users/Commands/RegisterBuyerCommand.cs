using Poshteh.Core.ApplicationService.Commands;

namespace PM.Identities.Core.Contract.Users.Commands;

/// <summary>
/// ثبت‌نام مستقل خریدار حقوقی. فقط حساب کاربری و وضعیت بررسی آن را می‌سازد؛
/// اطلاعات حقوقی شرکت خریدار (BuyerProfile, ENT-18) در ماژول Procurement است و توسط
/// هماهنگ‌کنندهٔ سطح Endpoint (نه این Handler) پس از موفقیت این Command ساخته می‌شود —
/// تا Core این ماژول به ماژول Procurement وابسته نشود.
/// </summary>
public sealed record RegisterBuyerCommand(
    string? UserName,
    string? Email,
    string PhoneNumber,
    string Password,
    string LegalNationalId,
    string? ConfirmPassword = null) : ICommand<long>;
