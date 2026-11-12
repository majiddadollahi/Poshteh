namespace PM.Identities.Core.Contract.Users.DTOs;

/// <summary>
/// فیلترهای فهرست مدیریت کاربران (TASK-0145) — عمداً مستقل از <see cref="UserListFilterDto"/>
/// چون این فهرست علاوه بر فیلترهای فعلی، فیلتر نوع کاربر و سازمان را هم اضافه می‌کند و خروجی آن
/// یک مدل اختصاصی (<see cref="UserListItemDto"/>) است، نه <see cref="Dto.UserDto"/> عمومی.
/// </summary>
public class UserManagementListFilterDto
{
    public string? Search { get; set; }
    public string? UserName { get; set; }
    public string? Email { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? PhoneNumber { get; set; }
    public bool? IsActive { get; set; }

    /// <summary>خانوادهٔ پنل (Panel/Buyer/Vendor)؛ خالی یعنی «همه».</summary>
    public UserTypeFamily? UserType { get; set; }

    /// <summary>جستجوی متنی (Contains) روی نام BuyerProfile/Company متعلق به کاربر.</summary>
    public string? OrganizationName { get; set; }

    /// <summary>
    /// تطابق دقیق (پس از trim) روی «کد ملی/شناسه ملی» مؤثر کاربر: شناسهٔ ملی/حقوقی BuyerProfile/Company
    /// متعلق به کاربر در صورت وجود سازمان واقعی، در غیر این صورت کد ملی شخصی خود کاربر (User.NationalId)
    /// — همان مقداری که ستون «کد ملی/شناسه ملی» فهرست نمایش می‌دهد؛ تا کاربر حقیقی هم با کد ملی پیدا شود.
    /// </summary>
    public string? NationalId { get; set; }

    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
