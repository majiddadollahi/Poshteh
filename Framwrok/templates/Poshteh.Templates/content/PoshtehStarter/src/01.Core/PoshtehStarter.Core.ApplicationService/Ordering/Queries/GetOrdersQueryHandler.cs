using Poshteh.Core.ApplicationService.Queries;
using Poshteh.Core.Domain;
using PoshtehStarter.Core.Contract.Common;
using PoshtehStarter.Core.Contract.Ordering;

namespace PoshtehStarter.Core.ApplicationService.Ordering.Queries;

/// <summary>
/// Validates the paging input, then delegates filtering, sorting and paging to the read-side repository.
/// </summary>
public sealed class GetOrdersQueryHandler(IOrderQueryRepository orders)
    : IQueryHandler<GetOrdersQuery, PagedResult<OrderSummaryDto>>
{
    public async Task<Result<PagedResult<OrderSummaryDto>>> Handle(GetOrdersQuery query, CancellationToken cancellationToken = default)
    {
        var criteria = query.Criteria;

        if (criteria.Page < 1)
        {
            return Result.Failure<PagedResult<OrderSummaryDto>>(OrderErrors.InvalidPage);
        }

        if (criteria.PageSize is < 1 or > PagedResult<OrderSummaryDto>.MaxPageSize)
        {
            return Result.Failure<PagedResult<OrderSummaryDto>>(OrderErrors.InvalidPageSize);
        }

        if (criteria.CreatedFrom > criteria.CreatedTo)
        {
            return Result.Failure<PagedResult<OrderSummaryDto>>(OrderErrors.InvalidDateRange);
        }

        return await orders.SearchAsync(criteria, cancellationToken);
    }
}
