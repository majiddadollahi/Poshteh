using PM.Identities.Core.Contract.Users.DTOs;
using Poshteh.Core.Domain;

namespace PM.Identities.Core.Contract.Users;

/// <summary>
/// Interface برای مدیریت User
/// </summary>
public interface IUserService
{
    // User Management
    Task<Result<UserDto>> CreateAsync(CreateUserDto dto, CancellationToken cancellationToken = default);
    Task<Result<UserDto>> UpdateAsync(long id, UpdateUserDto dto, CancellationToken cancellationToken = default);
    Task<Result<UserDto>> UpdateMyProfileAsync(long id, UpdateMyProfileDto dto, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(long id, CancellationToken cancellationToken = default);
    Task<Result<UserDto>> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<Result<UserDto>> GetByUserNameAsync(string userName, CancellationToken cancellationToken = default);
    Task<Result<UserDto>> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<Result<PagedUserListDto>> GetListAsync(UserListFilterDto filter, CancellationToken cancellationToken = default);
    Task<Result<PagedUserManagementListDto>> GetManagementListAsync(UserManagementListFilterDto filter, CancellationToken cancellationToken = default);
    Task<Result<List<UserSearchResultDto>>> SearchAsync(string text, CancellationToken cancellationToken = default);
    
    // Authentication
    Task<Result<AuthenticationResultDto>> LoginAsync(LoginDto dto, CancellationToken cancellationToken = default);
    Task<Result<AuthenticationResultDto>> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
    Task<Result> LogoutAsync(string refreshToken, CancellationToken cancellationToken = default);
    
    // Password Management
    Task<Result> ChangePasswordAsync(long userId, ChangePasswordDto dto, CancellationToken cancellationToken = default);
    Task<Result> AdminResetPasswordAsync(long userId, AdminResetPasswordDto dto, long? actorUserId = null, CancellationToken cancellationToken = default);
    Task<Result> RequestPasswordResetAsync(string email, CancellationToken cancellationToken = default);
    Task<Result> ResetPasswordAsync(ResetPasswordDto dto, CancellationToken cancellationToken = default);
    
    // Email Verification
    Task<Result> RequestEmailVerificationAsync(long userId, CancellationToken cancellationToken = default);
    Task<Result> VerifyEmailAsync(string token, CancellationToken cancellationToken = default);
    
    // Status Management
    Task<Result> ActivateUserAsync(long userId, long? actorUserId = null, CancellationToken cancellationToken = default);
    Task<Result> DeactivateUserAsync(long userId, long? actorUserId = null, CancellationToken cancellationToken = default);
}
