namespace PoshtehStarter.Core.Contract.Ordering;

public sealed record OrderDto(
    long Id,
    string CustomerName,
    DateTime? CreatedDate,
    DateTime? UpdateDate,
    decimal TotalAmount,
    IReadOnlyList<OrderItemDto> Items);

public sealed record OrderItemDto(long Id, string ProductName, decimal UnitPrice, int Quantity, decimal LineTotal);

public sealed record OrderSummaryDto(long Id, string CustomerName, DateTime? CreatedDate, int ItemCount, decimal TotalAmount);
