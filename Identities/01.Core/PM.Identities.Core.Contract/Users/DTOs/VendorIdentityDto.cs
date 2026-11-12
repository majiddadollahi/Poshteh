using PM.Identities.Core.Domain.Users;

namespace PM.Identities.Core.Contract.Users.DTOs;

public sealed record VendorIdentityDto(
    long Id,
    string FirstName,
    string LastName,
    string? NationalId,
    string? PhoneNumber,
    VendorReviewStatus Status,
    string? RejectionReason,
    string? CorrectionReason,
    DateTime? ReviewedAt,
    bool IsActive,
    string UserName,
    string? Email,
    DateTime? RegisteredAt,
    DateTime? LastLoginDate)
{
    public string FullName => $"{FirstName} {LastName}".Trim();
}
