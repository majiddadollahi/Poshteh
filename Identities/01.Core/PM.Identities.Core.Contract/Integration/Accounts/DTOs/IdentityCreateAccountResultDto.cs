namespace PM.Identities.Core.Contract.Integration.Accounts.DTOs;

public class IdentityCreateAccountResultDto
{
    public long UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
}
