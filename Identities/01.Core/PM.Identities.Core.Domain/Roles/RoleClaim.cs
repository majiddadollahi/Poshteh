using Microsoft.AspNetCore.Identity;

namespace PM.Identities.Core.Domain.Roles;

/// <summary>
/// Claims برای Role
/// </summary>
public class RoleClaim : IdentityRoleClaim<long>
{
    public Role Role { get; set; } = null!;
}

