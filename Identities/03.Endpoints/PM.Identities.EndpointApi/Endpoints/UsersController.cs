using PM.Identities.Core.Contract.Users.DTOs;
using PM.Identities.EndpointApi.Base;
using PM.Identities.Core.Contract.Roles;
using PM.Identities.Core.Contract.Users;
using PM.Identities.Core.Domain.Messages;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;
using PM.Identities.EndpointApi.Models;

namespace PM.Identities.EndpointApi.Endpoints;

[Route("identity/[controller]")]
[ApiController]
[Tags("Identity")]
public class UsersController : BaseApiController
{
    private readonly IUserService _userService;
    private readonly IRoleService _roleService;

    public UsersController(IUserService userService, IRoleService roleService)
    {
        _userService = userService;
        _roleService = roleService;
    }

    [HttpGet]
    public async Task<IActionResult> GetList([FromQuery] UserListFilterDto filter)
    {
        var result = await _userService.GetListAsync(filter);
        return HandleResult(result);
    }

    /// <summary>فهرست مدیریت کاربران (TASK-0145): کاربران پنل/خریدار/تأمین‌کننده یکپارچه، با نوع و سازمان واقعی.</summary>
    [HttpGet("management")]
    public async Task<IActionResult> GetManagementList([FromQuery] UserManagementListFilterDto filter, CancellationToken cancellationToken)
    {
        var result = await _userService.GetManagementListAsync(filter, cancellationToken);
        return HandleResult(result);
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string text)
    {
        var result = await _userService.SearchAsync(text);
        return HandleResult(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(long id)
    {
        var result = await _userService.GetByIdAsync(id);
        return HandleResult(result);
    }

    [HttpGet("{id}/roles")]
    public async Task<IActionResult> GetUserRoles(long id)
    {
        var result = await _roleService.GetUserRolesAsync(id);
        return HandleResult(result);
    }

    [HttpPost("{id}/roles/{roleId}")]
    public async Task<IActionResult> AddRoleToUser(long id, long roleId)
    {
        var result = await _roleService.AddUserToRoleAsync(id, roleId);
        return HandleResult(result, IdentityMessages.UserRoleAdded);
    }

    [HttpDelete("{id}/roles/{roleId}")]
    public async Task<IActionResult> RemoveRoleFromUser(long id, long roleId)
    {
        var result = await _roleService.RemoveUserFromRoleAsync(id, roleId);
        return HandleResult(result, IdentityMessages.UserRoleRemoved);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserDto dto)
    {
        var result = await _userService.CreateAsync(dto);
        return HandleResult(result, IdentityMessages.UserCreated);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(long id, [FromBody] UpdateUserDto dto)
    {
        var result = await _userService.UpdateAsync(id, dto);
        return HandleResult(result, IdentityMessages.UserUpdated);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(long id)
    {
        var result = await _userService.DeleteAsync(id);
        return HandleResult(result, IdentityMessages.UserDeleted);
    }

    [HttpPost("{id}/change-password")]
    public async Task<IActionResult> ChangePassword(long id, [FromBody] ChangePasswordDto dto)
    {
        var result = await _userService.ChangePasswordAsync(id, dto);
        return HandleResult(result, IdentityMessages.UserPasswordChanged);
    }

    /// <summary>
    /// Admin sets a new password for a user (no current password required).
    /// </summary>
    [HttpPost("{id}/reset-password")]
    public async Task<IActionResult> AdminResetPassword(long id, [FromBody] AdminResetPasswordDto dto)
    {
        var result = await _userService.AdminResetPasswordAsync(id, dto, GetActorUserId());
        return HandleResult(result, IdentityMessages.UserPasswordReset);
    }

    //[HttpPost("reset-password")]
    //[EnableRateLimiting("authentication")]
    //public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
    //{
    //    var result = await _userService.ResetPasswordAsync(dto);
    //    return HandleResult(result, IdentityMessages.UserPasswordReset);
    //}

    [HttpPost("request-password-reset")]
    [EnableRateLimiting("authentication")]
    public async Task<IActionResult> RequestPasswordReset([FromBody] RequestResetPasswordTokenDto dto)
    {
        // Always return the same public response. The reset token must only be delivered through
        // a verified out-of-band channel and must never be returned from this endpoint.
        var result = await _userService.RequestPasswordResetAsync(dto.Email);
        return HandleResult(result, IdentityMessages.UserPasswordResetRequested);
    }

    [HttpPost("{id}/activate")]
    public async Task<IActionResult> Activate(long id)
    {
        var result = await _userService.ActivateUserAsync(id, GetActorUserId());
        return HandleResult(result, IdentityMessages.UserActivated);
    }

    [HttpPost("{id}/deactivate")]
    public async Task<IActionResult> Deactivate(long id)
    {
        var result = await _userService.DeactivateUserAsync(id, GetActorUserId());
        return HandleResult(result, IdentityMessages.UserDeactivated);
    }

    private long? GetActorUserId() =>
        long.TryParse(User.FindFirstValue("UserId") ?? User.FindFirstValue(ClaimTypes.NameIdentifier), out var id)
            ? id
            : null;
}
