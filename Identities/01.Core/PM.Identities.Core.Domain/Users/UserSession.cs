using Poshteh.Core.Domain;

namespace PM.Identities.Core.Domain.Users;

/// <summary>
/// مدیریت Session های کاربر
/// </summary>
public class UserSession : BaseEntity
{
    public long UserId { get; set; }
    public User User { get; set; } = null!;
    
    public string SessionId { get; set; } = string.Empty;
    public string? DeviceInfo { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public DateTime LoginDate { get; set; } = DateTime.UtcNow;
    public DateTime? LogoutDate { get; set; }
    public DateTime LastActivityDate { get; set; } = DateTime.UtcNow;
    
    public bool IsActive => LogoutDate == null;
}

