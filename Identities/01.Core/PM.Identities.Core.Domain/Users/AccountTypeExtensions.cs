namespace PM.Identities.Core.Domain.Users;

public static class AccountTypeExtensions
{
    public static bool IsPanel(this AccountType value) =>
        value is AccountType.PanelOwner or AccountType.PanelAdmin or AccountType.PanelUser;

    public static bool IsBuyer(this AccountType value) =>
        value is AccountType.BuyerOwner or AccountType.BuyerAdmin or AccountType.BuyerUser;

    public static bool IsVendor(this AccountType value) =>
        value is AccountType.VendorOwner or AccountType.VendorAdmin or AccountType.VendorUser;

    public static bool IsOwner(this AccountType value) =>
        value is AccountType.PanelOwner or AccountType.BuyerOwner or AccountType.VendorOwner;
}
