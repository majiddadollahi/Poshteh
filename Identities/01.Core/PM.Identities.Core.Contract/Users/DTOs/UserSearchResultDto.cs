namespace PM.Identities.Core.Contract.Users.DTOs;

using PM.Identities.Core.Domain.Users;

public class UserSearchResultDto
{
    public long Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public AccountType AccountType { get; set; }
}
