using PM.Identities.Core.Contract.Integration.Users.DTOs;
using Poshteh.Core.Domain;
using PM.Identities.Core.Domain.Users;

namespace PM.Identities.Core.Contract.Integration.Users;

public interface IIdentityUserProfilesReaderContract
{
    Task<Result<IReadOnlyList<IdentityUserProfileDto>>> GetUserProfilesAsync(
        IReadOnlyCollection<long> userIds,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// همهٔ کاربران AccountType تأمین‌کننده را که VendorReviewStatus‌شان در <paramref name="reviewStatuses"/>
    /// است برمی‌گرداند (TASK-0066) — برای Matching تجمیعی که باید هر تأمین‌کنندهٔ واجد شرایط چرخهٔ عمر
    /// را دقیقاً یک‌بار برگرداند، نه فقط آن‌هایی که از قبل شناسه‌شان معلوم است.
    /// </summary>
    Task<Result<IReadOnlyList<IdentityUserProfileDto>>> GetVendorProfilesByReviewStatusAsync(
        IReadOnlyCollection<VendorReviewStatus> reviewStatuses,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(Result.Success<IReadOnlyList<IdentityUserProfileDto>>([]));

    /// <summary>
    /// تأمین‌کنندگان با حساب فعال و وضعیت تجاری Active. نام و شناسهٔ ملی، اگر مقدار داشته باشند،
    /// هم‌زمان روی پروفایل حقیقی اعمال می‌شوند. بدون معیار، همهٔ تأمین‌کنندگان فعال برمی‌گردند.
    /// </summary>
    Task<Result<IReadOnlyList<IdentityUserProfileDto>>> SearchActiveVendorProfilesAsync(
        string? name,
        string? nationalId,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(Result.Success<IReadOnlyList<IdentityUserProfileDto>>([]));

    Task<Result<EligibleSupplierProfilePageDto>> SearchEligibleSuppliersAsync(
        string? searchText,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(Result.Success(new EligibleSupplierProfilePageDto([], 0, Math.Max(pageNumber, 1), Math.Clamp(pageSize, 1, 50))));
}
