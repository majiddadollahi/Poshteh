using Microsoft.EntityFrameworkCore;
using PM.Identities.Core.Contract.Integration.Users;
using PM.Identities.Core.Domain.Users;
using PM.Identities.Infra.DAL.DbContext;

namespace PM.Identities.Core.ApplicationService.Integration.Users;

public sealed class IdentityAdminWorkReaderService(PMIdentityDbContext context) : IIdentityAdminWorkReaderContract
{
    public Task<int> CountPendingVendorProfilesAsync(CancellationToken cancellationToken = default) =>
        context.Users.AsNoTracking()
            .Where(u => AccountTypes.Vendor.Contains(u.AccountType) && u.VendorReviewStatus == VendorReviewStatus.Pending)
            .CountAsync(cancellationToken);
}
