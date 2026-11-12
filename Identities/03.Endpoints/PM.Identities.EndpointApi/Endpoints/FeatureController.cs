using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PM.Identities.Core.Contract.Features;
using PM.Identities.Core.Contract.Features.DTOs;
using PM.Identities.Core.Domain.Messages;
using PM.Identities.EndpointApi.Base;

namespace PM.Identities.EndpointApi.Endpoints;

[Route("identity/[controller]")]
[ApiController]
[Tags("Identity")]
public class FeatureController : BaseApiController
{
    private readonly IFeatureService _featureService;

    public FeatureController(IFeatureService featureService)
    {
        _featureService = featureService;
    }

    [HttpGet]
    public async Task<IActionResult> GetList() => HandleResult(await _featureService.GetFeatureListAsync());

    [HttpGet("tree")]
    public async Task<IActionResult> GetTree() => HandleResult(await _featureService.GetFeatureTreeAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(long id) => HandleResult(await _featureService.GetFeatureByIdAsync(id));

    [HttpGet("{id}/accesses")]
    public async Task<IActionResult> GetAccesses(long id, CancellationToken cancellationToken) =>
        HandleResult(await _featureService.GetFeatureAccessesAsync(id, cancellationToken));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateFeatureDto dto) =>
        HandleResult(await _featureService.CreateFeatureAsync(dto), IdentityMessages.FeatureCreated);

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(long id, [FromBody] UpdateFeatureDto dto) =>
        HandleResult(await _featureService.UpdateFeatureAsync(id, dto), IdentityMessages.FeatureUpdated);

    [HttpPut("{id}/accesses")]
    public async Task<IActionResult> ReplaceAccesses(long id, [FromBody] ReplaceFeatureAccessesDto dto) =>
        HandleResult(await _featureService.ReplaceFeatureAccessesAsync(id, dto), IdentityMessages.FeatureAccessUpdated);

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(long id) =>
        HandleResult(await _featureService.DeleteFeatureAsync(id), IdentityMessages.FeatureDeleted);
}
