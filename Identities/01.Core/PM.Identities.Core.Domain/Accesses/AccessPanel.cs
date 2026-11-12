namespace PM.Identities.Core.Domain.Accesses;

/// <summary>
/// TASK-0247: the panel an endpoint Access belongs to. Persisted on Identity.Accesses and applied
/// by the central PMAuthorizationFilter. <see cref="None"/> (unsynchronized/unknown) always denies.
/// </summary>
public enum AccessPanel : byte
{
    None = 0,
    /// <summary>Anonymous endpoints (login, registration, version, API metadata).</summary>
    Public = 1,
    /// <summary>Any authenticated user (own profile, notifications, shared catalog reads).</summary>
    Shared = 2,
    /// <summary>Admin panel: AccountType + full admin or Role→Feature→Access.</summary>
    Admin = 3,
    /// <summary>Buyer panel: buyer AccountType with a real BuyerProfile.</summary>
    Buyer = 4,
    /// <summary>Vendor panel: vendor AccountType with a VendorAccountKind.</summary>
    Vendor = 5,
    /// <summary>Either the Buyer or the Vendor panel (former ApprovedMarketplaceMember).</summary>
    Marketplace = 6
}
