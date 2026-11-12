using PM.Vendor.Core.Contract.Integration.Companies;
using PM.Identities.Core.Domain.Users;
namespace PM.Identities.Core.Contract.Users.DTOs;

public sealed record VendorOnboardingDto(long Id, string VendorType, string DisplayName, VendorReviewStatus VendorReviewStatus,
    string? RejectionReason, string? CorrectionReason, string FirstName, string LastName, string? NationalCode,
    string? PhoneNumber, string? Address, VendorCompanyData? Company, DateTime? RegisteredAt,
    DateTime? VendorProfileSubmittedAt, bool CanEdit, bool CanSubmit, IReadOnlyList<string> MissingFields);
