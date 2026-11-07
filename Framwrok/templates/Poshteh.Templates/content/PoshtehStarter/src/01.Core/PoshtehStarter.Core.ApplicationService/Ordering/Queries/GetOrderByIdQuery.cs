using Poshteh.Core.ApplicationService.Queries;
using PoshtehStarter.Core.Contract.Ordering;

namespace PoshtehStarter.Core.ApplicationService.Ordering.Queries;

public sealed record GetOrderByIdQuery(long OrderId) : IQuery<OrderDto>;
