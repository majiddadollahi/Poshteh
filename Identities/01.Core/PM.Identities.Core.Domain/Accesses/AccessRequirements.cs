namespace PM.Identities.Core.Domain.Accesses;

/// <summary>
/// TASK-0247: extra member-state conditions on top of <see cref="AccessPanel"/>; all set flags are
/// required. Only meaningful for the Buyer/Vendor/Marketplace panels.
/// </summary>
[Flags]
public enum AccessRequirements : byte
{
    None = 0,
    /// <summary>Buyer: BuyerProfile IsActive and Status "Active"; Vendor: VendorReviewStatus.Active.</summary>
    ApprovedMember = 1,
    /// <summary>Buyer: BuyerProfile IsActive (former ActiveBuyer policy).</summary>
    ActiveBuyer = 2
}
