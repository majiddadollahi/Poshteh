using PM.Identities.Core.Contract.Accesses.DTOs;
using PM.Identities.EndpointApi.Authorization;
using PM.Identities.EndpointApi.Base;
using PM.Identities.Core.Contract.Accesses;
using PM.Identities.Core.Domain.Messages;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;

namespace PM.Identities.EndpointApi.Endpoints;

[Route("identity/[controller]")]
[ApiController]
[Tags("Identity")]
public class AccessController : BaseApiController
{
    private readonly IAccessService _accessService;
    private readonly IApiDescriptionGroupCollectionProvider _apiDescriptions;

    public AccessController(IAccessService accessService, IApiDescriptionGroupCollectionProvider apiDescriptions)
    {
        _accessService = accessService;
        _apiDescriptions = apiDescriptions;
    }

    [HttpGet]
    public async Task<IActionResult> GetList([FromQuery] AccessListQuery query, CancellationToken cancellationToken)
    {
        return HandleResult(await _accessService.GetAccessListAsync(query, cancellationToken));
    }

    [HttpGet("tree")]
    public async Task<IActionResult> GetTree()
    {
        var definitions = EndpointAccessCatalogBuilder.Build(_apiDescriptions);
        return HandleResult(await _accessService.GetResourceTreeAsync(definitions));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(long id)
    {
        var result = await _accessService.GetAccessByIdAsync(id);
        return HandleResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAccessDto dto)
    {
        var result = await _accessService.CreateAccessAsync(dto);
        return HandleResult(result, IdentityMessages.AccessCreated);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(long id, [FromBody] UpdateAccessDto dto)
    {
        var result = await _accessService.UpdateAccessAsync(id, dto);
        return HandleResult(result, IdentityMessages.AccessUpdated);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(long id)
    {
        var result = await _accessService.DeleteAccessAsync(id);
        return HandleResult(result, IdentityMessages.AccessDeleted);
    }

}
