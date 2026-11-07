using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Poshteh.Core.ApplicationService.Commands;
using Poshteh.Core.ApplicationService.Queries;
using Poshteh.Endpoint.Api.Controllers;
using PoshtehStarter.Core.ApplicationService.Ordering.Commands;
using PoshtehStarter.Core.ApplicationService.Ordering.Queries;
using PoshtehStarter.Core.Contract.Common;
using PoshtehStarter.Core.Contract.Ordering;

namespace PoshtehStarter.Endpoint.Api.Controllers;

/// <summary>
/// CRUD for orders. Each action only maps HTTP input to a Command/Query; dispatch and the Result-to-HTTP
/// mapping (404 for "NotFound", 422 for "Validation", ...) are owned by <see cref="BaseController"/>.
/// </summary>
[Route("api/orders")]
[Produces("application/json")]
public sealed class OrdersController(ICommandDispatcher commandDispatcher, IQueryDispatcher queryDispatcher)
    : BaseController(commandDispatcher, queryDispatcher)
{
    /// <summary>Lists orders with optional filters, sorting and paging.</summary>
    [HttpGet]
    [ProducesResponseType<PagedResult<OrderSummaryDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public Task<IResult> GetList([FromQuery] OrderSearchRequest request, CancellationToken cancellationToken) =>
        ExecuteQuery(new GetOrdersQuery(request.ToCriteria()), cancellationToken);

    /// <summary>Gets one order with its line items.</summary>
    [HttpGet("{id:long}")]
    [ProducesResponseType<OrderDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public Task<IResult> GetById(long id, CancellationToken cancellationToken) =>
        ExecuteQuery(new GetOrderByIdQuery(id), cancellationToken);

    /// <summary>Creates an order and returns its id.</summary>
    [HttpPost]
    [ProducesResponseType<long>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public Task<IResult> Create([FromBody] OrderRequest request, CancellationToken cancellationToken) =>
        ExecuteCreate(new CreateOrderCommand(request.CustomerName, request.ToItems()), cancellationToken);

    /// <summary>Replaces the customer and the line items of an order.</summary>
    [HttpPut("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public Task<IResult> Update(long id, [FromBody] OrderRequest request, CancellationToken cancellationToken) =>
        ExecuteUpdate(new UpdateOrderCommand(id, request.CustomerName, request.ToItems()), cancellationToken);

    /// <summary>Deletes an order and its line items.</summary>
    [HttpDelete("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public Task<IResult> Delete(long id, CancellationToken cancellationToken) =>
        ExecuteDelete(new DeleteOrderCommand(id), cancellationToken);
}
