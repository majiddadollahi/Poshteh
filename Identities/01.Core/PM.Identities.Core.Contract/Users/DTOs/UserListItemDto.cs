namespace PM.Identities.Core.Contract.Users.DTOs;

/// <summary>
/// یک ردیف فهرست مدیریت کاربران (TASK-0145) — مدل خروجی اختصاصی این فهرست، عمداً جدا از
/// <see cref="UserDto"/> عمومی (مصرف‌شده در لاگین/جزئیات) تا تغییرات این فهرست آن قرارداد را نشکند.
/// </summary>
public sealed record UserListItemDto(
    long Id,
    string UserName,
    // شناسهٔ مؤثر: شناسهٔ ملی/حقوقی سازمان برای کاربر دارای سازمان واقعی، در غیر این صورت کد ملی شخص.
    string? EffectiveNationalId,
    // نام نمایشی: «مدیر [سازمان]» برای Owner/Admin سازمانی با سازمان واقعی، در غیر این صورت نام شخص.
    string DisplayName,
    UserTypeFamily UserType,
    string? OrganizationName,
    string? Email,
    string? PhoneNumber,
    bool IsActive);
