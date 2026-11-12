using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PM.Identities.Core.Contract.Roles.DTOs;
using Poshteh.Core.Domain;
using PM.Identities.Core.Contract.Accesses;
using PM.Identities.Core.Contract.Roles;
using PM.Identities.Infra.DAL.DbContext;
using PM.Identities.Core.Domain.Users;
using PM.Identities.Core.Domain.Errors;
using PM.Identities.Core.Domain.Roles;

namespace PM.Identities.Core.ApplicationService.Roles;

public class RoleService : IRoleService
{
    private readonly RoleManager<Role> _roleManager;
    private readonly UserManager<User> _userManager;
    private readonly PMIdentityDbContext _context;
    private readonly IAdminAccessCacheInvalidator _accessCacheInvalidator;

    public RoleService(
        RoleManager<Role> roleManager,
        UserManager<User> userManager,
        PMIdentityDbContext context,
        IAdminAccessCacheInvalidator accessCacheInvalidator)
    {
        _roleManager = roleManager;
        _userManager = userManager;
        _context = context;
        _accessCacheInvalidator = accessCacheInvalidator;
    }

    public async Task<Result<RoleDto>> CreateAsync(CreateRoleDto dto, CancellationToken cancellationToken = default)
    {
        var existingRole = await _roleManager.FindByNameAsync(dto.Name);
        if (existingRole != null)
        {
            return Result.Failure<RoleDto>(IdentityErrors.RoleAlreadyExists(dto.Name));
        }

        var role = new Role
        {
            Name = dto.Name,
            Description = dto.Description,
            IsSystemRole = false,
            IsActive = dto.IsActive
        };

        var result = await _roleManager.CreateAsync(role);
        if (!result.Succeeded)
        {
            var errorMessage = string.Join(", ", result.Errors.Select(e => e.Description));
            return Result.Failure<RoleDto>(IdentityErrors.RoleCreationFailed(errorMessage));
        }

        return Result.Success(MapToDto(role));
    }

    public async Task<Result<RoleDto>> UpdateAsync(long id, UpdateRoleDto dto, CancellationToken cancellationToken = default)
    {
        var role = await _roleManager.FindByIdAsync(id.ToString());
        if (role == null)
        {
            return Result.Failure<RoleDto>(IdentityErrors.RoleNotFound(id));
        }

        if (role.IsSystemRole)
        {
            return Result.Failure<RoleDto>(IdentityErrors.SystemRoleCannotBeModified);
        }

        role.Name = dto.Name;
        role.Description = dto.Description;

        var result = await _roleManager.UpdateAsync(role);
        if (!result.Succeeded)
        {
            var errorMessage = string.Join(", ", result.Errors.Select(e => e.Description));
            return Result.Failure<RoleDto>(IdentityErrors.RoleUpdateFailed(errorMessage));
        }

        return Result.Success(MapToDto(role));
    }

    public async Task<Result> DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        var role = await _roleManager.FindByIdAsync(id.ToString());
        if (role == null)
        {
            return Result.Failure(IdentityErrors.RoleNotFound(id));
        }

        if (role.IsSystemRole)
        {
            return Result.Failure(IdentityErrors.SystemRoleCannotBeDeleted);
        }

        // Capture members before cascade delete removes UserRoles.
        var affectedUserIds = await _context.UserRoles.AsNoTracking()
            .Where(ur => ur.RoleId == id)
            .Select(ur => ur.UserId)
            .Distinct()
            .ToListAsync(cancellationToken);

        var result = await _roleManager.DeleteAsync(role);
        if (!result.Succeeded)
        {
            var errorMessage = string.Join(", ", result.Errors.Select(e => e.Description));
            return Result.Failure(IdentityErrors.RoleDeleteFailed(errorMessage));
        }

        foreach (var userId in affectedUserIds)
        {
            _accessCacheInvalidator.InvalidateUser(userId);
        }

        return Result.Success();
    }

    public async Task<Result<RoleDto>> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var role = await _roleManager.FindByIdAsync(id.ToString());
        if (role == null)
        {
            return Result.Failure<RoleDto>(IdentityErrors.RoleNotFound(id));
        }
        return Result.Success(MapToDto(role));
    }

    public async Task<Result<RoleDto>> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        var role = await _roleManager.FindByNameAsync(name);
        if (role == null)
        {
            return Result.Failure<RoleDto>(IdentityErrors.RoleNotFound(name));
        }
        return Result.Success(MapToDto(role));
    }

    public async Task<Result<List<RoleDto>>> GetListAsync(RoleListFilterDto? filter = null, CancellationToken cancellationToken = default)
    {
        filter ??= new RoleListFilterDto();
        var query = _roleManager.Roles.AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Name))
        {
            var name = filter.Name.Trim();
            query = query.Where(r => r.Name != null && r.Name.Contains(name));
        }

        if (filter.IsActive.HasValue)
        {
            query = query.Where(r => r.IsActive == filter.IsActive.Value);
        }

        var roles = await query
            .OrderBy(r => r.Name)
            .ToListAsync(cancellationToken);

        return Result.Success(roles.Select(MapToDto).ToList());
    }

    public async Task<Result> ActivateRoleAsync(long id, CancellationToken cancellationToken = default)
    {
        var role = await _roleManager.FindByIdAsync(id.ToString());
        if (role == null)
        {
            return Result.Failure(IdentityErrors.RoleNotFound(id));
        }

        if (role.IsActive)
        {
            return Result.Failure(IdentityErrors.RoleAlreadyActive);
        }

        role.IsActive = true;
        var result = await _roleManager.UpdateAsync(role);
        if (!result.Succeeded)
        {
            var errorMessage = string.Join(", ", result.Errors.Select(e => e.Description));
            return Result.Failure(IdentityErrors.RoleActivationFailed(errorMessage));
        }

        return Result.Success();
    }

    public async Task<Result> DeactivateRoleAsync(long id, CancellationToken cancellationToken = default)
    {
        var role = await _roleManager.FindByIdAsync(id.ToString());
        if (role == null)
        {
            return Result.Failure(IdentityErrors.RoleNotFound(id));
        }

        if (role.IsSystemRole)
        {
            return Result.Failure(IdentityErrors.SystemRoleCannotBeDeactivated);
        }

        if (!role.IsActive)
        {
            return Result.Failure(IdentityErrors.RoleAlreadyInactive);
        }

        role.IsActive = false;
        var result = await _roleManager.UpdateAsync(role);
        if (!result.Succeeded)
        {
            var errorMessage = string.Join(", ", result.Errors.Select(e => e.Description));
            return Result.Failure(IdentityErrors.RoleDeactivationFailed(errorMessage));
        }

        return Result.Success();
    }

    public async Task<Result> AddUserToRoleAsync(long userId, long roleId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            return Result.Failure(IdentityErrors.UserNotFound(userId));
        }

        var role = await _roleManager.FindByIdAsync(roleId.ToString());
        if (role == null)
        {
            return Result.Failure(IdentityErrors.RoleNotFound(roleId));
        }

        if (!role.IsActive)
        {
            return Result.Failure(IdentityErrors.RoleInactive);
        }

        var result = await _userManager.AddToRoleAsync(user, role.Name!);
        if (!result.Succeeded)
        {
            var errorMessage = string.Join(", ", result.Errors.Select(e => e.Description));
            return Result.Failure(IdentityErrors.AddUserToRoleFailed(errorMessage));
        }

        _accessCacheInvalidator.InvalidateUser(userId);
        return Result.Success();
    }

    public async Task<Result> RemoveUserFromRoleAsync(long userId, long roleId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            return Result.Failure(IdentityErrors.UserNotFound(userId));
        }

        var role = await _roleManager.FindByIdAsync(roleId.ToString());
        if (role == null)
        {
            return Result.Failure(IdentityErrors.RoleNotFound(roleId));
        }

        var result = await _userManager.RemoveFromRoleAsync(user, role.Name!);
        if (!result.Succeeded)
        {
            var errorMessage = string.Join(", ", result.Errors.Select(e => e.Description));
            return Result.Failure(IdentityErrors.RemoveUserFromRoleFailed(errorMessage));
        }

        _accessCacheInvalidator.InvalidateUser(userId);
        return Result.Success();
    }

    public async Task<Result<List<RoleDto>>> GetUserRolesAsync(long userId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            return Result.Failure<List<RoleDto>>(IdentityErrors.UserNotFound(userId));
        }

        var roleNames = await _userManager.GetRolesAsync(user);
        var roles = await _roleManager.Roles
            .Where(r => roleNames.Contains(r.Name!))
            .ToListAsync(cancellationToken);

        return Result.Success(roles.Select(MapToDto).ToList());
    }

    public async Task<Result<List<RoleUserDto>>> GetRoleUsersAsync(long roleId, CancellationToken cancellationToken = default)
    {
        var role = await _roleManager.FindByIdAsync(roleId.ToString());
        if (role == null)
        {
            return Result.Failure<List<RoleUserDto>>(IdentityErrors.RoleNotFound(roleId));
        }

        var users = await _context.Users
            .Where(u => u.UserRoles.Any(ur => ur.RoleId == roleId))
            .ToListAsync(cancellationToken);

        return Result.Success(users.Select(MapToRoleUserDto).ToList());
    }

    public async Task<Result> AddClaimToRoleAsync(long roleId, string claimType, string claimValue, CancellationToken cancellationToken = default)
    {
        var role = await _roleManager.FindByIdAsync(roleId.ToString());
        if (role == null)
        {
            return Result.Failure(IdentityErrors.RoleNotFound(roleId));
        }

        var result = await _roleManager.AddClaimAsync(role, new System.Security.Claims.Claim(claimType, claimValue));
        if (!result.Succeeded)
        {
            var errorMessage = string.Join(", ", result.Errors.Select(e => e.Description));
            return Result.Failure(IdentityErrors.AddClaimToRoleFailed(errorMessage));
        }

        return Result.Success();
    }

    public async Task<Result> RemoveClaimFromRoleAsync(long roleId, string claimType, CancellationToken cancellationToken = default)
    {
        var role = await _roleManager.FindByIdAsync(roleId.ToString());
        if (role == null)
        {
            return Result.Failure(IdentityErrors.RoleNotFound(roleId));
        }

        var claims = await _roleManager.GetClaimsAsync(role);
        var claimToRemove = claims.FirstOrDefault(c => c.Type == claimType);
        if (claimToRemove != null)
        {
            var result = await _roleManager.RemoveClaimAsync(role, claimToRemove);
            if (!result.Succeeded)
            {
                var errorMessage = string.Join(", ", result.Errors.Select(e => e.Description));
                return Result.Failure(IdentityErrors.RemoveClaimFromRoleFailed(errorMessage));
            }
        }

        return Result.Success();
    }

    public async Task<Result<List<RoleClaimDto>>> GetRoleClaimsAsync(long roleId, CancellationToken cancellationToken = default)
    {
        var role = await _roleManager.FindByIdAsync(roleId.ToString());
        if (role == null)
        {
            return Result.Failure<List<RoleClaimDto>>(IdentityErrors.RoleNotFound(roleId));
        }

        var claims = await _roleManager.GetClaimsAsync(role);
        var result = claims.Select(c => new RoleClaimDto
        {
            Id = 0, // Claim ID از Identity نیست
            ClaimType = c.Type,
            ClaimValue = c.Value
        }).ToList();

        return Result.Success(result);
    }

    private static RoleDto MapToDto(Role role)
    {
        return new RoleDto
        {
            Id = role.Id,
            Name = role.Name ?? string.Empty,
            Description = role.Description,
            IsSystemRole = role.IsSystemRole,
            IsActive = role.IsActive
        };
    }

    private static RoleUserDto MapToRoleUserDto(User user)
    {
        return new RoleUserDto
        {
            Id = user.Id,
            UserName = user.UserName ?? string.Empty,
            Email = user.Email ?? string.Empty,
            FirstName = user.FirstName,
            LastName = user.LastName,
            FullName = user.DisplayName,
            PhoneNumber = user.PhoneNumber,
            IsActive = user.IsActive
        };
    }
}
