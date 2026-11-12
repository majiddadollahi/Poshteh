namespace PM.Identities.Core.Contract.Integration.Accounts.DTOs;

public class IdentityAccountWithRolesAndAccessesDto
{
    public long UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public List<IdentityAccountRoleDto> Roles { get; set; } = new();
    public List<IdentityAccountAccessDto> Accesses { get; set; } = new();
}
