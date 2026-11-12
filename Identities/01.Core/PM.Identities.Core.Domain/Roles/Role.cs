using PM.Identities.Core.Domain.Users;
using Microsoft.AspNetCore.Identity;

namespace PM.Identities.Core.Domain.Roles;

/// <summary>
/// Role Entity - System-wide Roles از ASP.NET Identity استفاده می‌کند
/// </summary>
public class Role : IdentityRole<long>
{
    public string? Description { get; set; }
    public bool IsSystemRole { get; set; } = false; // نقش‌های سیستمی قابل حذف نیستند
    public bool IsActive { get; set; } = true;
    
    // Navigation Properties
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public ICollection<RoleClaim> RoleClaims { get; set; } = new List<RoleClaim>();
    public ICollection<RoleFeature> RoleFeatures { get; set; } = new List<RoleFeature>();
}

