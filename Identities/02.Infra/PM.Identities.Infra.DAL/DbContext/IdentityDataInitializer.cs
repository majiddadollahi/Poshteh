using PM.Identities.Core.Domain.Roles;
using Microsoft.AspNetCore.Identity;

namespace PM.Identities.Infra.DAL.DbContext;

/// <summary>
/// کلاس برای Seed کردن داده‌های اولیه Identity
/// </summary>
public class IdentityDataInitializer
{
    private readonly RoleManager<Role> _roleManager;

    private static readonly (string Name, string Description)[] SystemRoles =
    [
        ("developer", "Developer Role"),
        ("admin", "Administrator Role"),
        ("user", "User Role")
    ];

    public IdentityDataInitializer(RoleManager<Role> roleManager)
    {
        _roleManager = roleManager;
    }

    /// <summary>
    /// Seed کردن تمام داده‌های اولیه
    /// </summary>
    public async Task SeedDataAsync()
    {
        await SeedRolesAsync();
    }

    /// <summary>
    /// ایجاد نقش‌های سیستمی
    /// </summary>
    private async Task SeedRolesAsync()
    {
        foreach (var roleData in SystemRoles)
        {
            if (!await _roleManager.RoleExistsAsync(roleData.Name))
            {
                var role = new Role
                {
                    Name = roleData.Name,
                    NormalizedName = roleData.Name.ToUpperInvariant(),
                    Description = roleData.Description,
                    IsSystemRole = true,
                    IsActive = true
                };

                var result = await _roleManager.CreateAsync(role);
                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    throw new InvalidOperationException($"Failed to create role '{roleData.Name}': {errors}");
                }
            }
            else
            {
                // اگر نقش وجود دارد، مطمئن شوید که IsSystemRole = true است
                var existingRole = await _roleManager.FindByNameAsync(roleData.Name);
                if (existingRole != null && !existingRole.IsSystemRole)
                {
                    existingRole.IsSystemRole = true;
                    existingRole.Description = roleData.Description;
                    existingRole.IsActive = true;
                    await _roleManager.UpdateAsync(existingRole);
                }
                else if (existingRole != null && !existingRole.IsActive)
                {
                    existingRole.IsActive = true;
                    await _roleManager.UpdateAsync(existingRole);
                }
            }
        }
    }

}
