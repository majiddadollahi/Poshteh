using PM.Identities.Core.Contract.Users.DTOs;
using Poshteh.Core.ApplicationService.Commands;
namespace PM.Identities.Core.Contract.Users.Commands;
public sealed record SubmitVendorProfileCommand(long UserId) : ICommand<VendorOnboardingDto>;
