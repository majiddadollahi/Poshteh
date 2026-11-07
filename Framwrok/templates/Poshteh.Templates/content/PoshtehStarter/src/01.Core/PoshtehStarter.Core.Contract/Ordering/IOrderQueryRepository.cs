using PoshtehStarter.Core.Contract.Common;

namespace PoshtehStarter.Core.Contract.Ordering;

/// <summary>
/// Read-side port for orders. Implemented over the no-tracking query context, never over the aggregate.
/// </summary>
public interface IOrderQueryRepository
{
    Task<OrderDto?> GetByIdAsync(long id, CancellationToken cancellationToken = default);

    Task<PagedResult<OrderSummaryDto>> SearchAsync(OrderSearchCriteria criteria, CancellationToken cancellationToken = default);
}
