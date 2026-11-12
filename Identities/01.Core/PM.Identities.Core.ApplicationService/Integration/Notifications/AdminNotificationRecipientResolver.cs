using Microsoft.AspNetCore.Identity;
using PM.Identities.Core.Domain.Users;
using PM.Platform.Core.Contract.Notifications;

namespace PM.Identities.Core.ApplicationService.Integration.Notifications;

public sealed class AdminNotificationRecipientResolver(UserManager<User> users)
    : IAdminNotificationRecipientResolver
{
    public async Task<IReadOnlyCollection<long>> GetAdminRecipientUserIdsAsync(
        CancellationToken cancellationToken = default)
    {
        var admins = await users.GetUsersInRoleAsync("admin");
        return admins.Where(user => user.IsActive).Select(user => user.Id).Distinct().ToArray();
    }
}
