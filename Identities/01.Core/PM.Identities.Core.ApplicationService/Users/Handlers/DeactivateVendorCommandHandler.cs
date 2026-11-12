using Microsoft.EntityFrameworkCore;
using PM.Identities.Core.Contract.Users.Commands;
using PM.Identities.Core.Contract.Users.DTOs;
using PM.Identities.Core.Domain.Users;
using PM.Identities.Infra.DAL.DbContext;
using Poshteh.Core.ApplicationService.Commands;
using Poshteh.Core.Domain;
using Poshteh.Shared.Kernel.Time;
namespace PM.Identities.Core.ApplicationService.Users.Handlers;
public sealed class DeactivateVendorCommandHandler(PMIdentityDbContext db, VendorWorkflow workflow, IClock clock)
    : ICommandHandler<DeactivateVendorCommand, VendorOnboardingDto>
{
    public async Task<Result<VendorOnboardingDto>> Handle(DeactivateVendorCommand c, CancellationToken cancellationToken = default)
    {
        if (c.UserId <= 0 || c.AdminUserId <= 0) return Result.Failure<VendorOnboardingDto>(VendorProfileRules.Forbidden);
        await using var tx = await workflow.LockAsync(c.UserId, cancellationToken);
        var u = await db.Users.SingleOrDefaultAsync(u => u.Id == c.UserId && AccountTypes.Vendor.Contains(u.AccountType), cancellationToken);
        if (u is null) return Result.Failure<VendorOnboardingDto>(new Error("Vendor.NotFound", "تأمین‌کننده یافت نشد."));
        if (u.VendorReviewStatus != VendorReviewStatus.Active) return Result.Failure<VendorOnboardingDto>(VendorProfileRules.Conflict);
        db.Add(new VendorReviewEntry { VendorUserId = u.Id, ActorUserId = c.AdminUserId, FromStatus = u.VendorReviewStatus,
            ToStatus = VendorReviewStatus.Inactive, OccurredAt = clock.UtcNow.UtcDateTime });
        u.VendorReviewStatus = VendorReviewStatus.Inactive; u.VendorReviewedAt = clock.UtcNow.UtcDateTime; u.VendorReviewedByUserId = c.AdminUserId;
        await db.SaveChangesAsync(cancellationToken);
        if (tx is not null) await tx.CommitAsync(cancellationToken);
        return await workflow.DtoAsync(u, cancellationToken);
    }
}
