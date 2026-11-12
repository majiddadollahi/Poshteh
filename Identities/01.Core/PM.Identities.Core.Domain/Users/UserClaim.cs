using Microsoft.AspNetCore.Identity;

namespace PM.Identities.Core.Domain.Users;

/// <summary>
/// Claims اضافی برای User
/// </summary>
public class UserClaim : IdentityUserClaim<long>
{
    public User User { get; set; } = null!;
}

