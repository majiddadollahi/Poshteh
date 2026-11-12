namespace PM.Identities.Core.Domain.Users;

/// <summary>
/// Single authoritative business-status lifecycle for a vendor account (TASK-0020). Registration
/// starts at <see cref="Pending"/>; admin approval is the transition to <see cref="Active"/> — there
/// is no independent "activity" status anywhere else (Company.IsActive was retired in favor of this).
/// <see cref="Active"/> keeps its historical persisted value of 1 (previously named "Approved") so
/// existing rows need no value remap, only a C# rename. <see cref="Inactive"/> is a new value used
/// only by admin deactivation of a previously <see cref="Active"/> vendor.
/// </summary>
public enum VendorReviewStatus
{
    Pending = 0,
    Active = 1,
    Rejected = 2,
    PendingCorrection = 3,
    ProfileIncomplete = 4,
    Inactive = 5
}
