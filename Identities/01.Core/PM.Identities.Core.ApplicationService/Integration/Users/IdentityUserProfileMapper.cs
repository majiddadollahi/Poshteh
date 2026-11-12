using PM.Identities.Core.Contract.Integration.Users.DTOs;
using PM.Identities.Core.Domain.Users;

namespace PM.Identities.Core.ApplicationService.Integration.Users;

internal static class IdentityUserProfileMapper
{
    public static IdentityUserProfileDto Map(User user) => new()
    {
        UserId = user.Id,
        UserName = user.UserName ?? string.Empty,
        Email = user.Email ?? string.Empty,
        PhoneNumber = user.PhoneNumber,
        FirstName = user.FirstName,
        LastName = user.LastName,
        FullName = user.DisplayName,
        NationalId = user.NationalId,
        IsActive = user.IsActive,
        EmailConfirmed = user.EmailConfirmed,
        LastLoginDate = user.LastLoginDate,
        ProfilePictureUrl = user.ProfilePictureUrl,
        ReviewStatus = user.VendorReviewStatus,
        RejectionReason = user.VendorRejectionReason,
        CorrectionReason = user.VendorCorrectionReason
    };
}
