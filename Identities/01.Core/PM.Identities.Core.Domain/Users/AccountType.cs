using System.Text.Json.Serialization;

namespace PM.Identities.Core.Domain.Users;

/// <summary>
/// Classifies which application panel an account belongs to and the account's
/// organizational kind. It is identity metadata and never grants permissions.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AccountType
{
    PanelOwner = 1,
    PanelAdmin = 2,
    PanelUser = 3,
    BuyerOwner = 4,
    BuyerAdmin = 5,
    BuyerUser = 6,
    VendorOwner = 7,
    VendorAdmin = 8,
    VendorUser = 9,
}
