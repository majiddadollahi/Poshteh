using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PM.Identities.Core.ApplicationService.Users.Validators;
using PM.Identities.Core.Contract.Users.Commands;
using PM.Identities.Core.Contract.Users.DTOs;
using PM.Identities.Core.Domain.Users;
using PM.Identities.Infra.DAL.DbContext;
using Poshteh.Core.ApplicationService.Commands;
using Poshteh.Core.Domain;
using Poshteh.Shared.Kernel.Time;
namespace PM.Identities.Core.ApplicationService.Users.Handlers;
public sealed class ReviewVendorCommandHandler(PMIdentityDbContext db, VendorWorkflow workflow, IClock clock,
    ILogger<ReviewVendorCommandHandler> logger) : ICommandHandler<ReviewVendorCommand, VendorOnboardingDto>
{
    public async Task<Result<VendorOnboardingDto>> Handle(ReviewVendorCommand c, CancellationToken cancellationToken = default)
    {
        var valid = new ReviewVendorCommandValidator().Validate(c);
        if (valid.IsFailure) return Result.Failure<VendorOnboardingDto>(valid.Error);
        await using var tx = await workflow.LockAsync(c.UserId, cancellationToken);
        var u = await db.Users.SingleOrDefaultAsync(u => u.Id == c.UserId && AccountTypes.Vendor.Contains(u.AccountType), cancellationToken);
        if (u is null) return Result.Failure<VendorOnboardingDto>(new Error("Vendor.NotFound", "تأمین‌کننده یافت نشد."));
        if (u.VendorReviewStatus != VendorReviewStatus.Pending) return Result.Failure<VendorOnboardingDto>(VendorProfileRules.Conflict);
        db.Add(new VendorReviewEntry { VendorUserId = u.Id, ActorUserId = c.ReviewerId, FromStatus = u.VendorReviewStatus,
            ToStatus = c.TargetStatus, OccurredAt = clock.UtcNow.UtcDateTime, Reason = c.Reason?.Trim() });
        u.VendorReviewStatus = c.TargetStatus; u.VendorReviewedAt = clock.UtcNow.UtcDateTime; u.VendorReviewedByUserId = c.ReviewerId;
        u.VendorRejectionReason = c.TargetStatus == VendorReviewStatus.Rejected ? c.Reason?.Trim() : null;
        u.VendorCorrectionReason = c.TargetStatus == VendorReviewStatus.PendingCorrection ? c.Reason?.Trim() : null;
        await db.SaveChangesAsync(cancellationToken);
        if (tx is not null) await tx.CommitAsync(cancellationToken);
        // VendorsController.Review در مسیر موفق این خروجی را دور می‌ریزد و پاسخ را دوباره از GetById می‌سازد؛
        // پس خطای enrichment بعد از commit موفق (مثلاً لغو/timeout درخواست) نباید تغییر وضعیتی را که
        // واقعاً ثبت شده، به شکست ظاهری برای کاربر تبدیل کند.
        try { return await workflow.DtoAsync(u, cancellationToken); }
        catch (Exception exception)
        {
            logger.LogError(exception, "DtoAsync enrichment failed after committing vendor review status change for user {UserId}", u.Id);
            return new VendorOnboardingDto(u.Id, "individual", u.DisplayName, u.VendorReviewStatus, u.VendorRejectionReason,
                u.VendorCorrectionReason, u.FirstName, u.LastName, u.NationalId, u.PhoneNumber, null, null,
                u.RegisteredAt, u.VendorProfileSubmittedAt, false, false, []);
        }
    }
}
