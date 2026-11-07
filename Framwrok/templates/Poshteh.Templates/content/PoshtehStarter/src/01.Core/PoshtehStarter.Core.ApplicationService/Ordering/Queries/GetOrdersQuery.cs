using Poshteh.Core.ApplicationService.Queries;
using PoshtehStarter.Core.Contract.Common;
using PoshtehStarter.Core.Contract.Ordering;

namespace PoshtehStarter.Core.ApplicationService.Ordering.Queries;

public sealed record GetOrdersQuery(OrderSearchCriteria Criteria) : IQuery<PagedResult<OrderSummaryDto>>;
