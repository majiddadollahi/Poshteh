using Poshteh.Core.ApplicationService.Queries;
using Poshteh.Core.Contract;
using Poshteh.Core.Domain;
using Poshteh.Sample.Ordering.Domain;

namespace Poshteh.Sample.Ordering.Application.Queries;

/// <summary>
/// Reads through <see cref="ICommandRepository{TEntity}"/> rather than a dedicated read port: this sample has
/// not yet migrated to a separate read-side store, so it reuses the write-side repository's get-by-id for this
/// one lookup. <c>Poshteh.Infra.Data.Sql.Query</c> now ships <c>IReadRepository&lt;T&gt;</c> — a real read model
/// mapped onto its own no-tracking context is the natural next step for this handler, not done here to keep
/// this sample's diff focused on proving the CQRS/Event/Exception-handling wiring end to end.
/// </summary>
public sealed class GetOrderByIdQueryHandler(ICommandRepository<Order> orders) : IQueryHandler<GetOrderByIdQuery, OrderDto>
{
    public async Task<Result<OrderDto>> Handle(GetOrderByIdQuery query, CancellationToken cancellationToken = default)
    {
        var order = await orders.GetByIdAsync(query.OrderId, cancellationToken);

        if (order is null)
        {
            return Result.Failure<OrderDto>(new Error("Order.NotFound"));
        }

        return new OrderDto(
            order.Id,
            order.CustomerName,
            order.TotalAmount,
            order.Items
                .Select(item => new OrderItemDto(item.Id, item.ProductName, item.UnitPrice, item.Quantity, item.LineTotal))
                .ToList());
    }
}
