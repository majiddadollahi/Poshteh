using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PM.Identities.Core.Contract.Features;
using PM.Identities.Core.Domain.Messages;
using PM.Identities.EndpointApi.Base;

namespace PM.Identities.EndpointApi.Endpoints;

[Route("identity/role-feature")]
[ApiController]
[Tags("Identity")]
public class RoleFeatureController : BaseApiController
{
    private readonly IRoleFeatureService _service;

    public RoleFeatureController(IRoleFeatureService service)
    {
        _service = service;
    }

    [HttpGet("{roleId}/features")]
    public async Task<IActionResult> GetRoleFeatures(long roleId) =>
        HandleResult(await _service.GetRoleFeaturesAsync(roleId));

    [HttpGet("{roleId}/all-features-with-selection")]
    public async Task<IActionResult> GetAllFeaturesWithSelection(long roleId) =>
        HandleResult(await _service.GetAllFeaturesWithSelectionAsync(roleId));

    [HttpPost("{roleId}/features/{featureId}")]
    public async Task<IActionResult> AddFeature(long roleId, long featureId) =>
        HandleResult(await _service.AddFeatureToRoleAsync(roleId, featureId), IdentityMessages.RoleFeatureAdded);

    [HttpDelete("{roleId}/features/{featureId}")]
    public async Task<IActionResult> RemoveFeature(long roleId, long featureId) =>
        HandleResult(await _service.RemoveFeatureFromRoleAsync(roleId, featureId), IdentityMessages.RoleFeatureRemoved);

    [HttpPost("{roleId}/features/{featureId}/tree")]
    public async Task<IActionResult> AddFeatureTree(long roleId, long featureId) =>
        HandleResult(await _service.AddFeatureTreeToRoleAsync(roleId, featureId), IdentityMessages.RoleFeatureTreeAdded);

    [HttpDelete("{roleId}/features/{featureId}/tree")]
    public async Task<IActionResult> RemoveFeatureTree(long roleId, long featureId) =>
        HandleResult(await _service.RemoveFeatureTreeFromRoleAsync(roleId, featureId), IdentityMessages.RoleFeatureTreeRemoved);
}
