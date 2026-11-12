using PM.Identities.Core.Contract.Users.DTOs;
using PM.Identities.EndpointApi.Base;
using PM.Identities.Core.Contract.Accesses;
using PM.Identities.Core.Contract.Auth;
using PM.Identities.Core.Contract.Users;
using PM.Identities.Core.Domain.Messages;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace PM.Identities.EndpointApi.Endpoints;

[Route("identity/me")]
[ApiController]
[Tags("Identity")]
public class MyAdminAccessesController : BaseApiController
{
    private readonly IAdminAccessQueryService _adminAccessQueryService;
    private readonly IUserService _userService;

    public MyAdminAccessesController(
        IAdminAccessQueryService adminAccessQueryService,
        IUserService userService)
    {
        _adminAccessQueryService = adminAccessQueryService;
        _userService = userService;
    }

    /// <summary>
    /// Current authenticated user's profile (self-service; not admin-gated).
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetMyProfile()
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var result = await _userService.GetByIdAsync(userId);
        return HandleResult(result);
    }

    [HttpPut]
    public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateMyProfileDto dto)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();
        return HandleResult(await _userService.UpdateMyProfileAsync(userId, dto));
    }

    /// <summary>
    /// Change the current user's password (requires current password).
    /// </summary>
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangeMyPassword([FromBody] ChangePasswordDto dto)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var result = await _userService.ChangePasswordAsync(userId, dto);
        return HandleResult(result, IdentityMessages.UserPasswordChanged);
    }

    /// <summary>
    /// Access keys of the current user for Admin Panel UI bootstrap.
    /// Authenticated callers may always read their own grant state.
    /// </summary>
    [HttpGet("accesses")]
    public async Task<IActionResult> GetMyAccesses()
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var result = await _adminAccessQueryService.GetMyAccessesAsync(userId);
        return HandleResult(result);
    }

    /// <summary>
    /// Lightweight poll endpoint: when AccessVersion increases, the UI should reload accesses.
    /// </summary>
    [HttpGet("access-version")]
    public async Task<IActionResult> GetMyAccessVersion()
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var result = await _adminAccessQueryService.GetMyAccessVersionAsync(userId);
        return HandleResult(result);
    }

    private bool TryGetUserId(out long userId)
    {
        var userIdClaim = User.FindFirstValue(MahtaJwtClaimTypes.UserId)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        return long.TryParse(userIdClaim, out userId);
    }
}
