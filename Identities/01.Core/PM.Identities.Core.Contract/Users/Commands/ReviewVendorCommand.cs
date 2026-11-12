using PM.Identities.Core.Contract.Users.DTOs;
using PM.Identities.Core.Domain.Users;
using Poshteh.Core.ApplicationService.Commands;
namespace PM.Identities.Core.Contract.Users.Commands;
public sealed record ReviewVendorCommand(long UserId, long ReviewerId, VendorReviewStatus ExpectedStatus,
    VendorReviewStatus TargetStatus, string? Reason) : ICommand<VendorOnboardingDto>;
