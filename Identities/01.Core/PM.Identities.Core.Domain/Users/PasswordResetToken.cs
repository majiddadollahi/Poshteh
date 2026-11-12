using Poshteh.Core.Domain;

namespace PM.Identities.Core.Domain.Users;

/// <summary>
/// Token برای Reset Password
/// </summary>
public class PasswordResetToken : BaseEntity
{
    public long UserId { get; set; }
    public User User { get; set; } = null!;
    
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public bool IsUsed { get; set; } = false;
    public DateTime? UsedAt { get; set; }
    
    public bool IsValid => !IsUsed && DateTime.UtcNow < ExpiresAt;
}

