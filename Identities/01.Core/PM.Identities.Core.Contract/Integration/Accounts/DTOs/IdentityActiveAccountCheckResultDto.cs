namespace PM.Identities.Core.Contract.Integration.Accounts.DTOs;

public class IdentityActiveAccountCheckResultDto
{
    public bool Exists { get; set; }
    public bool IsActive { get; set; }
    public long? UserId { get; set; }
    public string? UserName { get; set; }
}
