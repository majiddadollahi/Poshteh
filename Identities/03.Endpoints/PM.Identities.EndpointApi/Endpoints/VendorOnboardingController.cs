using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using PM.Identities.Core.Contract.Users.Commands;
using PM.Identities.Core.Contract.Users.Queries;
using PM.Identities.EndpointApi.Base;
using Poshteh.Core.ApplicationService.Commands;
using Poshteh.Core.ApplicationService.Queries;
namespace PM.Identities.EndpointApi.Endpoints;
[Route("vendor/Onboarding")]
public sealed class VendorOnboardingController : BaseApiController
{
    private readonly ICommandDispatcher _commands;

    public VendorOnboardingController(IQueryDispatcher queries, ICommandDispatcher commands)
        : base(commands, queries)
    {
        _commands = commands;
    }

    private long UserId => long.Parse(User.FindFirstValue("UserId") ?? User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    [HttpGet("me")]
    public Task<IActionResult> Get(CancellationToken ct) =>
        ExecuteQuery(new GetVendorOnboardingQuery(UserId), ct);
    [HttpPut("me")]
    public Task<IActionResult> Update(VendorProfileUpdateRequest r, CancellationToken ct) =>
        UpdateCommand(new UpdateVendorProfileCommand(UserId, r.FirstName, r.LastName, r.NationalCode, r.PhoneNumber, r.Address, r.Company), ct);
    [HttpPost("submit-for-approval")]
    [HttpPost("resubmit")]
    public async Task<IActionResult> Submit(CancellationToken ct) =>
        HandleResult(await _commands.Send(new SubmitVendorProfileCommand(UserId), ct));
}
