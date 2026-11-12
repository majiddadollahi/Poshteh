namespace PM.Identities.Core.Contract.Integration.Accounts.DTOs;

public class IdentityAccountAccessDto
{
    public long AccessId { get; set; }
    public string Key { get; set; } = string.Empty;
    public string? PersianTitle { get; set; }
}
