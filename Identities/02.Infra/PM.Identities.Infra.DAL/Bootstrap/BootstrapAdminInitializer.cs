using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PM.Identities.Core.Domain.Roles;
using PM.Identities.Core.Domain.Users;
using PM.Identities.Infra.DAL.DbContext;

namespace PM.Identities.Infra.DAL.Bootstrap;

public sealed class BootstrapAdminInitializer(
    UserManager<User> userManager,
    RoleManager<Role> roleManager,
    PMIdentityDbContext dbContext,
    IOptions<BootstrapAdminOptions> options,
    ILogger<BootstrapAdminInitializer> logger)
{
    private static readonly string[] RequiredRoles = ["admin", "developer"];

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        var settings = options.Value;
        if (!settings.Enabled)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(settings.Password))
        {
            throw new InvalidOperationException(
                "Bootstrap admin is enabled, but BootstrapAdmin:Password is missing.");
        }

        if (string.IsNullOrWhiteSpace(settings.UserName) ||
            string.IsNullOrWhiteSpace(settings.FirstName) ||
            string.IsNullOrWhiteSpace(settings.LastName))
        {
            throw new InvalidOperationException("Bootstrap admin identity settings are invalid.");
        }

        var roles = new List<Role>(RequiredRoles.Length);
        foreach (var roleName in RequiredRoles)
        {
            var role = await roleManager.FindByNameAsync(roleName);
            if (role is null || !role.IsActive)
            {
                throw new InvalidOperationException(
                    $"Required bootstrap role '{roleName}' is missing or inactive.");
            }

            roles.Add(role);
        }

        var user = await userManager.FindByNameAsync(settings.UserName);
        if (user is null)
        {
            if (await dbContext.Users.AnyAsync(cancellationToken))
            {
                throw new InvalidOperationException(
                    "Bootstrap admin cannot be created because the identity database is not empty.");
            }

            user = new User
            {
                UserName = settings.UserName.Trim(),
                FirstName = settings.FirstName.Trim(),
                LastName = settings.LastName.Trim(),
                AccountType = AccountType.PanelOwner,
                IsActive = true,
                RegisteredAt = DateTime.UtcNow
            };

            var createResult = await userManager.CreateAsync(user, settings.Password);
            EnsureSucceeded(createResult, "create the bootstrap admin account");

            var addRolesResult = await userManager.AddToRolesAsync(user, RequiredRoles);
            if (!addRolesResult.Succeeded)
            {
                var deleteResult = await userManager.DeleteAsync(user);
                if (!deleteResult.Succeeded)
                {
                    logger.LogCritical(
                        "Bootstrap admin role assignment and compensating account deletion both failed for {UserName}.",
                        settings.UserName);
                }

                EnsureSucceeded(addRolesResult, "assign bootstrap admin roles");
            }

            logger.LogInformation("Bootstrap admin account {UserName} was created.", settings.UserName);
            return;
        }

        if (user.AccountType != AccountType.PanelOwner)
        {
            user.AccountType = AccountType.PanelOwner;
            EnsureSucceeded(await userManager.UpdateAsync(user), "set the bootstrap account type");
        }

        var currentRoles = await userManager.GetRolesAsync(user);
        var missingRoles = RequiredRoles
            .Except(currentRoles, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (missingRoles.Length > 0)
        {
            var addRolesResult = await userManager.AddToRolesAsync(user, missingRoles);
            EnsureSucceeded(addRolesResult, "ensure bootstrap admin roles");
        }

        logger.LogInformation(
            "Bootstrap admin account {UserName} already exists; required roles were verified.",
            settings.UserName);
    }

    private static void EnsureSucceeded(IdentityResult result, string operation)
    {
        if (result.Succeeded)
        {
            return;
        }

        var errors = string.Join(", ", result.Errors.Select(error => error.Description));
        throw new InvalidOperationException($"Failed to {operation}: {errors}");
    }
}
