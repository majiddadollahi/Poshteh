using Poshteh.Core.Domain;

namespace PM.Identities.Core.Domain.Users;

/// <summary>
/// مدیریت Refresh Tokens
/// </summary>
public class RefreshToken : BaseEntity
{
    public long UserId { get; set; }
    public User User { get; set; } = null!;
    
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public DateTime? RevokedAt { get; set; }
    public string? RevokedBy { get; set; }
    public string? ReplacedByToken { get; set; }
    
    public bool IsActive => RevokedAt == null && DateTime.UtcNow < ExpiresAt;

    public bool IsRevoked { get; set; }
}

