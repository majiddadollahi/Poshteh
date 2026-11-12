namespace PM.Identities.Core.Contract.Roles.DTOs;

public class RoleClaimDto
{
    public long Id { get; set; }
    public string ClaimType { get; set; } = string.Empty;
    public string ClaimValue { get; set; } = string.Empty;
}

