using Microsoft.AspNetCore.Identity;

namespace PM.Identities.Core.Domain.Users;

/// <summary>
/// External Logins (Google, Microsoft, etc.)
/// </summary>
public class UserLogin : IdentityUserLogin<long>
{
    public User User { get; set; } = null!;
}

