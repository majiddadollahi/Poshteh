namespace PM.Identities.Core.Contract.Integration.Accounts.DTOs;

public class IdentityActiveAccountCheckRequestDto
{
    public long? UserId { get; set; }
    public string? UserName { get; set; }
    public string? Email { get; set; }
    public string? NationalId { get; set; }
}
