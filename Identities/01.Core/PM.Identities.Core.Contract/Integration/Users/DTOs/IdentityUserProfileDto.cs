namespace PM.Identities.Core.Contract.Integration.Users.DTOs;

using PM.Identities.Core.Domain.Users;

public class IdentityUserProfileDto
{
    public long UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? NationalId { get; set; }
    public bool IsActive { get; set; }
    public bool EmailConfirmed { get; set; }
    public DateTime? LastLoginDate { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public VendorReviewStatus ReviewStatus { get; set; }
    public string? RejectionReason { get; set; }
    public string? CorrectionReason { get; set; }
}
