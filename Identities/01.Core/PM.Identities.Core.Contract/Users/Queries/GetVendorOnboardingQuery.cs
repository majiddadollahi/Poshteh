using PM.Identities.Core.Contract.Users.DTOs;
using Poshteh.Core.ApplicationService.Queries;
namespace PM.Identities.Core.Contract.Users.Queries;
public sealed record GetVendorOnboardingQuery(long UserId) : IQuery<VendorOnboardingDto>;
