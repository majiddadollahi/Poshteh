using System.Text.Json.Serialization;

namespace PM.Identities.Core.Contract.Users.DTOs;

/// <summary>
/// خانوادهٔ پنل یک حساب (مستقل از جایگاه Owner/Admin/User درون آن) — برای فیلتر و نمایش «نوع
/// کاربر» در فهرست مدیریت کاربران؛ از روی <see cref="PM.Identities.Core.Domain.Users.AccountType"/>
/// دیتابیسی مشتق می‌شود، نه از Username یا Role.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum UserTypeFamily
{
    Panel = 1,
    Buyer = 2,
    Vendor = 3,
}
