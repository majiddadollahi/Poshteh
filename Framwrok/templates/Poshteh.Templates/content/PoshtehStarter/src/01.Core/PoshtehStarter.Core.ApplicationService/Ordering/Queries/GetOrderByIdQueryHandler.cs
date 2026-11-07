using Poshteh.Core.ApplicationService.Queries;
using Poshteh.Core.Domain;
using PoshtehStarter.Core.Contract.Ordering;

namespace PoshtehStarter.Core.ApplicationService.Ordering.Queries;

public sealed class GetOrderByIdQueryHandler(IOrderQueryRepository orders) : IQueryHandler<GetOrderByIdQuery, OrderDto>
{
    public async Task<Result<OrderDto>> Handle(GetOrderByIdQuery query, CancellationToken cancellationToken = default)
    {
        var order = await orders.GetByIdAsync(query.OrderId, cancellationToken);

        return order is null ? Result.Failure<OrderDto>(OrderErrors.NotFound) : order;
    }
}
