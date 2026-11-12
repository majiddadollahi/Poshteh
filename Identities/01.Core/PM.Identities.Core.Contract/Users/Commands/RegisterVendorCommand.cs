using Poshteh.Core.ApplicationService.Commands;
namespace PM.Identities.Core.Contract.Users.Commands;
public sealed record RegisterVendorCommand(string? UserName, string? Email, string? PhoneNumber, string Password,
    string? FirstName, string? LastName, bool IsLegalEntity = false, string? NationalCode = null,
    string? LegalNationalId = null, string? CompanyName = null, string? Address = null,
    string? ConfirmPassword = null, string? CompanyPhoneNumber = null) : ICommand<long>;
