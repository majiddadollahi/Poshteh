namespace PM.Identities.Core.Contract.Integration.Accounts.DTOs;

using PM.Identities.Core.Domain.Users;

public class IdentityCreateAccountRequestDto
{
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? NationalId { get; set; }
    public AccountType AccountType { get; set; }
}
