namespace PM.Identities.Core.Domain.Users;

public static class AccountTypes
{
    public static readonly AccountType[] Panel =
        [AccountType.PanelOwner, AccountType.PanelAdmin, AccountType.PanelUser];

    public static readonly AccountType[] Buyer =
        [AccountType.BuyerOwner, AccountType.BuyerAdmin, AccountType.BuyerUser];

    public static readonly AccountType[] Vendor =
        [AccountType.VendorOwner, AccountType.VendorAdmin, AccountType.VendorUser];
}
