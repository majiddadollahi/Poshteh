using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Poshteh.Core.ApplicationService.Commands;
using Poshteh.Core.ApplicationService.Queries;
using Poshteh.Core.Contract.Localization;
using Poshteh.Endpoint.Api.Results;

namespace Poshteh.Endpoint.Api.Controllers;

/// <summary>
/// Every controller in this project derives from this instead of talking to
/// <see cref="ICommandDispatcher"/>/<see cref="IQueryDispatcher"/> directly, so Command/Query dispatch and
/// Result-to-HTTP mapping stay in one place.
/// </summary>
[ApiController]
public abstract class BaseController : ControllerBase
{
    private readonly ICommandDispatcher _commandDispatcher;
    private readonly IQueryDispatcher _queryDispatcher;

    protected BaseController(ICommandDispatcher commandDispatcher, IQueryDispatcher queryDispatcher)
    {
        _commandDispatcher = commandDispatcher;
        _queryDispatcher = queryDispatcher;
    }

    protected Task<IResult> ExecuteCreate<TResponse>(
        ICommand<TResponse> command,
        CancellationToken cancellationToken = default) =>
        DispatchCommand(command, cancellationToken);

    protected Task<IResult> ExecuteUpdate<TResponse>(
        ICommand<TResponse> command,
        CancellationToken cancellationToken = default) =>
        DispatchCommand(command, cancellationToken);

    protected Task<IResult> ExecuteDelete<TResponse>(
        ICommand<TResponse> command,
        CancellationToken cancellationToken = default) =>
        DispatchCommand(command, cancellationToken);

    protected async Task<IResult> ExecuteQuery<TResponse>(
        IQuery<TResponse> query,
        CancellationToken cancellationToken = default)
    {
        var result = await _queryDispatcher.Send(query, cancellationToken);
        return result.ToApiResult(HttpContext.RequestServices.GetRequiredService<IMessageLocalizer>());
    }

    private async Task<IResult> DispatchCommand<TResponse>(
        ICommand<TResponse> command,
        CancellationToken cancellationToken)
    {
        var result = await _commandDispatcher.Send(command, cancellationToken);
        return result.ToApiResult(HttpContext.RequestServices.GetRequiredService<IMessageLocalizer>());
    }
}
