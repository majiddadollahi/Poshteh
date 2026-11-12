using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PM.Identities.Core.Contract.Users;
using PM.Identities.Core.Domain.Users;
using PM.Procurement.Core.Contract.Integration.BuyerProfiles;
using PM.Vendor.Core.Contract.Integration.Companies;
namespace PM.Identities.Core.ApplicationService.Users;
public sealed class VendorLoginResolver(UserManager<User> users, IVendorOnboardingStore profiles,
    IBuyerProfileReaderContract buyerProfiles)
{
    public async Task<User?> ResolveAsync(string identifier, CancellationToken ct)
    {
        var value = IranianIdentifiers.Normalize(identifier);
        var user = await users.FindByNameAsync(identifier.Trim());
        if (user is not null) return user;
        if (IranianIdentifiers.IsPersonal(value))
            return await users.Users.SingleOrDefaultAsync(u => u.NationalId == value &&
                u.VendorAccountKind == VendorAccountKind.Individual &&
                AccountTypes.Vendor.Contains(u.AccountType), ct);
        if (value.Length != 11 || value.Any(character => character is < '0' or > '9')) return null;

        var vendorId = await profiles.FindLegalOwnerAsync(value, ct);
        if (vendorId is not null)
            return await users.Users.SingleOrDefaultAsync(u => u.Id == vendorId &&
                AccountTypes.Vendor.Contains(u.AccountType), ct);

        var buyerId = await buyerProfiles.FindUserIdByLegalIdAsync(value, ct);
        return buyerId is null ? null : await users.Users.SingleOrDefaultAsync(u => u.Id == buyerId &&
            AccountTypes.Buyer.Contains(u.AccountType), ct);
    }
}
