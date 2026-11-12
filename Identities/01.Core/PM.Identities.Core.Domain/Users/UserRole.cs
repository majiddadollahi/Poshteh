using PM.Identities.Core.Domain.Roles;
using Microsoft.AspNetCore.Identity;

namespace PM.Identities.Core.Domain.Users;

/// <summary>
/// رابطه User و Role (System-wide)
/// </summary>
public class UserRole : IdentityUserRole<long>
{
    public User User { get; set; } = null!;
    public Role Role { get; set; } = null!;
}
