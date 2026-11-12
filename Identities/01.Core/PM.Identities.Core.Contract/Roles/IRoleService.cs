using PM.Identities.Core.Contract.Roles.DTOs;
using Poshteh.Core.Domain;

namespace PM.Identities.Core.Contract.Roles;

/// <summary>
/// Interface برای مدیریت Role
/// </summary>
public interface IRoleService
{
    Task<Result<RoleDto>> CreateAsync(CreateRoleDto dto, CancellationToken cancellationToken = default);
    Task<Result<RoleDto>> UpdateAsync(long id, UpdateRoleDto dto, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(long id, CancellationToken cancellationToken = default);
    Task<Result<RoleDto>> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<Result<RoleDto>> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<Result<List<RoleDto>>> GetListAsync(RoleListFilterDto? filter = null, CancellationToken cancellationToken = default);
    
    Task<Result> ActivateRoleAsync(long id, CancellationToken cancellationToken = default);
    Task<Result> DeactivateRoleAsync(long id, CancellationToken cancellationToken = default);
    
    // User-Role Management
    Task<Result> AddUserToRoleAsync(long userId, long roleId, CancellationToken cancellationToken = default);
    Task<Result> RemoveUserFromRoleAsync(long userId, long roleId, CancellationToken cancellationToken = default);
    Task<Result<List<RoleDto>>> GetUserRolesAsync(long userId, CancellationToken cancellationToken = default);
    Task<Result<List<RoleUserDto>>> GetRoleUsersAsync(long roleId, CancellationToken cancellationToken = default);
    
    // Role Claims
    Task<Result> AddClaimToRoleAsync(long roleId, string claimType, string claimValue, CancellationToken cancellationToken = default);
    Task<Result> RemoveClaimFromRoleAsync(long roleId, string claimType, CancellationToken cancellationToken = default);
    Task<Result<List<RoleClaimDto>>> GetRoleClaimsAsync(long roleId, CancellationToken cancellationToken = default);
}

