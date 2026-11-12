using PM.Identities.Core.Contract.Users.Commands;
using PM.Identities.Core.Domain.Users;
using Poshteh.Core.ApplicationService.Behaviors;
using Poshteh.Core.Domain;
namespace PM.Identities.Core.ApplicationService.Users.Validators;
public sealed class ReviewVendorCommandValidator : IValidator<ReviewVendorCommand>
{
    public Result Validate(ReviewVendorCommand r)
    {
        if (r.UserId <= 0 || r.ReviewerId <= 0) return Result.Failure(VendorProfileRules.Forbidden);
        if (r.ExpectedStatus != VendorReviewStatus.Pending || r.TargetStatus is not (VendorReviewStatus.Active or VendorReviewStatus.Rejected or VendorReviewStatus.PendingCorrection))
            return Result.Failure(VendorProfileRules.Conflict);
        if (r.TargetStatus != VendorReviewStatus.Active && (string.IsNullOrWhiteSpace(r.Reason) || r.Reason.Length > 1000))
            return Result.Failure(new Error("Vendor.ReasonRequired", "دلیل بررسی الزامی و حداکثر ۱۰۰۰ نویسه است."));
        return Result.Success();
    }
}
