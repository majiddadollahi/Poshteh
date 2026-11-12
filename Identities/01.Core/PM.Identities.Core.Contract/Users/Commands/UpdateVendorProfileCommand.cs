using PM.Vendor.Core.Contract.Integration.Companies;
using PM.Identities.Core.Contract.Users.DTOs;
using Poshteh.Core.ApplicationService.Commands;
namespace PM.Identities.Core.Contract.Users.Commands;
public sealed record UpdateVendorProfileCommand(long UserId, string FirstName, string LastName, string? NationalCode,
    string? PhoneNumber, string? Address, VendorCompanyData? Company) : ICommand<VendorOnboardingDto>;
