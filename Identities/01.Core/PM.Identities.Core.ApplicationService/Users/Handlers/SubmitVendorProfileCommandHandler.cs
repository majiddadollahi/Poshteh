using PM.Identities.Core.Contract.Users.Commands;
using PM.Identities.Core.Contract.Users.DTOs;
using PM.Identities.Core.Domain.Users;
using PM.Identities.Infra.DAL.DbContext;
using PM.Vendor.Core.Contract.Integration.Companies;
using Poshteh.Core.ApplicationService.Commands;
using Poshteh.Core.Domain;
using Poshteh.Shared.Kernel.Time;
namespace PM.Identities.Core.ApplicationService.Users.Handlers;
public sealed class SubmitVendorProfileCommandHandler(PMIdentityDbContext db, IVendorOnboardingStore profiles,
    VendorWorkflow workflow, IClock clock) : ICommandHandler<SubmitVendorProfileCommand, VendorOnboardingDto>
{
    public async Task<Result<VendorOnboardingDto>> Handle(SubmitVendorProfileCommand c, CancellationToken cancellationToken = default)
    {
        await using var tx = await workflow.LockAsync(c.UserId, cancellationToken);
        var u = await workflow.UserAsync(c.UserId, cancellationToken);
        if (u is null) return Result.Failure<VendorOnboardingDto>(VendorProfileRules.Forbidden);
        if (!VendorProfileRules.CanEdit(u.VendorReviewStatus)) return Result.Failure<VendorOnboardingDto>(VendorProfileRules.Conflict);
        var missing = VendorProfileRules.Missing(u, await profiles.GetAsync(u.Id, cancellationToken));
        if (missing.Count > 0) return Result.Failure<VendorOnboardingDto>(new Error("Vendor.ProfileIncomplete", "اطلاعات لازم: " + string.Join("، ", missing)));
        db.Add(new VendorReviewEntry { VendorUserId = u.Id, ActorUserId = u.Id, FromStatus = u.VendorReviewStatus,
            ToStatus = VendorReviewStatus.Pending, OccurredAt = clock.UtcNow.UtcDateTime, Reason = u.VendorRejectionReason ?? u.VendorCorrectionReason });
        u.VendorReviewStatus = VendorReviewStatus.Pending; u.VendorProfileSubmittedAt = clock.UtcNow.UtcDateTime;
        u.VendorRejectionReason = null; u.VendorCorrectionReason = null;
        await db.SaveChangesAsync(cancellationToken);
        if (tx is not null) await tx.CommitAsync(cancellationToken);
        return await workflow.DtoAsync(u, cancellationToken);
    }
}
