using PM.Identities.Core.Contract.Users.DTOs;
using Poshteh.Core.ApplicationService.Commands;
namespace PM.Identities.Core.Contract.Users.Commands;

public sealed record ActivateVendorCommand(long UserId, long AdminUserId) : ICommand<VendorOnboardingDto>;
