using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PM.Identities.Core.Contract.Users.Commands;
using PM.Identities.Core.Contract.Users.DTOs;
using PM.Identities.Core.Domain.Errors;
using PM.Identities.Core.Domain.Users;
using Poshteh.Core.ApplicationService.Commands;
using Poshteh.Core.Domain;

namespace PM.Identities.Core.ApplicationService.Users.Handlers;

public sealed class RequestVendorCorrectionCommandHandler(UserManager<User> userManager)
    : ICommandHandler<RequestVendorCorrectionCommand, VendorIdentityDto>
{
    public async Task<Result<VendorIdentityDto>> Handle(RequestVendorCorrectionCommand command, CancellationToken cancellationToken = default)
    {
        var user = await userManager.Users
            .Where(u => AccountTypes.Vendor.Contains(u.AccountType))
            .SingleOrDefaultAsync(u => u.Id == command.VendorId, cancellationToken);

        if (user is null)
        {
            return Result.Failure<VendorIdentityDto>(IdentityErrors.UserNotFound(command.VendorId));
        }

        if (user.VendorReviewStatus != command.ExpectedStatus)
        {
            return Result.Failure<VendorIdentityDto>(IdentityErrors.VendorReviewStatusConflict);
        }

        user.VendorReviewStatus = VendorReviewStatus.PendingCorrection;
        user.VendorCorrectionReason = command.CorrectionReason.Trim();
        user.VendorRejectionReason = null;
        user.VendorReviewedAt = DateTime.UtcNow;
        user.VendorReviewedByUserId = command.ReviewerId;

        var result = await userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            var errorMessage = string.Join(", ", result.Errors.Select(e => e.Description));
            return Result.Failure<VendorIdentityDto>(IdentityErrors.UserUpdateFailed(errorMessage));
        }

        return new VendorIdentityDto(user.Id, user.FirstName, user.LastName, user.NationalId, user.PhoneNumber,
            user.VendorReviewStatus, user.VendorRejectionReason, user.VendorCorrectionReason, user.VendorReviewedAt,
            user.IsActive, user.UserName ?? string.Empty, user.Email, user.RegisteredAt, user.LastLoginDate);
    }
}
