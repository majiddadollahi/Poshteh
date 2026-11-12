using PM.Identities.Core.Contract.Users.DTOs;
using PM.Identities.Core.Domain.Users;
using Poshteh.Core.ApplicationService.Queries;

namespace PM.Identities.Core.Contract.Users.Queries;

/// <summary>
/// فیلترهای مستقل فهرست تأمین‌کنندگان (نام، کدملی، موبایل، وضعیت). فیلتر «نوع تأمین‌کننده»
/// (حقیقی/حقوقی) یک مفهوم بیرونی (ماژول Vendor) است؛ چون هستهٔ این ماژول به Vendor وابسته
/// نیست (همان تصمیم RegisterVendorCommand/BL-038)، هماهنگ‌کنندهٔ سطح Endpoint مجموعهٔ
/// کاربرهای حقوقی را از قبل از طریق ICompanyReaderContract حل می‌کند و اینجا فقط یک
/// مجموعهٔ سادهٔ Id پاس داده می‌شود.
/// </summary>
public sealed record GetVendorListQuery(
    string? Name = null,
    string? NationalId = null,
    string? PhoneNumber = null,
    VendorReviewStatus? Status = null,
    bool? IsLegalVendor = null,
    IReadOnlySet<long>? LegalVendorUserIds = null,
    int PageNumber = 1,
    int PageSize = 20) : IQuery<VendorIdentityPageDto>;
