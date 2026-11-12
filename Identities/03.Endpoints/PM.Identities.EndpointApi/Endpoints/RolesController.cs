using PM.Identities.Core.Contract.Roles.DTOs;
using PM.Identities.EndpointApi.Base;
using PM.Identities.Core.Contract.Roles;
using PM.Identities.Core.Domain.Messages;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace PM.Identities.EndpointApi.Endpoints;

[Route("identity/[controller]")]
[ApiController]
[Tags("Identity")]
public class RolesController : BaseApiController
{
    private readonly IRoleService _roleService;

    public RolesController(IRoleService roleService)
    {
        _roleService = roleService;
    }

    [HttpGet]
    public async Task<IActionResult> GetList([FromQuery] RoleListFilterDto filter)
    {
        var result = await _roleService.GetListAsync(filter);
        return HandleResult(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(long id)
    {
        var result = await _roleService.GetByIdAsync(id);
        return HandleResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateRoleDto dto)
    {
        var result = await _roleService.CreateAsync(dto);
        return HandleResult(result, IdentityMessages.RoleCreated);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(long id, [FromBody] UpdateRoleDto dto)
    {
        var result = await _roleService.UpdateAsync(id, dto);
        return HandleResult(result, IdentityMessages.RoleUpdated);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(long id)
    {
        var result = await _roleService.DeleteAsync(id);
        return HandleResult(result, IdentityMessages.RoleDeleted);
    }

    [HttpPost("{id}/activate")]
    public async Task<IActionResult> Activate(long id)
    {
        var result = await _roleService.ActivateRoleAsync(id);
        return HandleResult(result, IdentityMessages.RoleActivated);
    }

    [HttpPost("{id}/deactivate")]
    public async Task<IActionResult> Deactivate(long id)
    {
        var result = await _roleService.DeactivateRoleAsync(id);
        return HandleResult(result, IdentityMessages.RoleDeactivated);
    }

    [HttpPost("{roleId}/users/{userId}")]
    public async Task<IActionResult> AddUserToRole(long roleId, long userId)
    {
        var result = await _roleService.AddUserToRoleAsync(userId, roleId);
        return HandleResult(result, IdentityMessages.RoleUserAdded);
    }

    [HttpDelete("{roleId}/users/{userId}")]
    public async Task<IActionResult> RemoveUserFromRole(long roleId, long userId)
    {
        var result = await _roleService.RemoveUserFromRoleAsync(userId, roleId);
        return HandleResult(result, IdentityMessages.RoleUserRemoved);
    }

    [HttpGet("users/{userId}")]
    public async Task<IActionResult> GetUserRoles(long userId)
    {
        var result = await _roleService.GetUserRolesAsync(userId);
        return HandleResult(result);
    }

    [HttpGet("{roleId}/users")]
    public async Task<IActionResult> GetRoleUsers(long roleId)
    {
        var result = await _roleService.GetRoleUsersAsync(roleId);
        return HandleResult(result);
    }
}
