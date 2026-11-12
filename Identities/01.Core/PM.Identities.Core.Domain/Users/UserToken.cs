using Microsoft.AspNetCore.Identity;

namespace PM.Identities.Core.Domain.Users;

/// <summary>
/// Tokens برای User
/// </summary>
public class UserToken : IdentityUserToken<long>
{
    public DateTime? ExpiresAt { get; set; }
    public bool IsRevoked { get; set; } = false;
    public User User { get; set; } = null!;
}

