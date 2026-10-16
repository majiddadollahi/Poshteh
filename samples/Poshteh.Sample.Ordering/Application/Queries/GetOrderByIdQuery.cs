using Poshteh.Core.ApplicationService.Queries;

namespace Poshteh.Sample.Ordering.Application.Queries;

public sealed record GetOrderByIdQuery(long OrderId) : IQuery<OrderDto>;

public sealed record OrderDto(long Id, string CustomerName, decimal TotalAmount, IReadOnlyList<OrderItemDto> Items);

public sealed record OrderItemDto(long Id, string ProductName, decimal UnitPrice, int Quantity, decimal LineTotal);
