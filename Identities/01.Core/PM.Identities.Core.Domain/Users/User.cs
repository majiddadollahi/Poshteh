using Microsoft.AspNetCore.Identity;

namespace PM.Identities.Core.Domain.Users;

/// <summary>
/// User Entity - از ASP.NET Identity استفاده می‌کند
/// </summary>
public class User : IdentityUser<long>
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? NationalId { get; set; }
    public AccountType AccountType { get; set; }
    public VendorAccountKind? VendorAccountKind { get; set; }
    public DateTime? RegisteredAt { get; set; }
    public bool IsActive { get; set; } = true;
    public VendorReviewStatus VendorReviewStatus { get; set; }
    public string? VendorRejectionReason { get; set; }
    public string? VendorCorrectionReason { get; set; }
    public DateTime? VendorProfileSubmittedAt { get; set; }
    public DateTime? VendorReviewedAt { get; set; }
    public long? VendorReviewedByUserId { get; set; }
    public DateTime? LastLoginDate { get; set; }
    public string? ProfilePictureUrl { get; set; }
    
    // Navigation Properties
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    public ICollection<UserSession> UserSessions { get; set; } = new List<UserSession>();
    public ICollection<PasswordResetToken> PasswordResetTokens { get; set; } = new List<PasswordResetToken>();
    public ICollection<EmailVerificationToken> EmailVerificationTokens { get; set; } = new List<EmailVerificationToken>();
    
    public string FullName => $"{FirstName} {LastName}".Trim();

    public string DisplayName => !string.IsNullOrWhiteSpace(FullName)
        ? FullName
        : AccountType is AccountType.BuyerOwner or AccountType.VendorOwner
            ? "مدیر"
            : "کاربر";
}
