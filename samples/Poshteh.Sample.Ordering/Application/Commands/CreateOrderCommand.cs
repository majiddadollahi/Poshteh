using Poshteh.Core.ApplicationService.Commands;

namespace Poshteh.Sample.Ordering.Application.Commands;

public sealed record CreateOrderCommand(string CustomerName, IReadOnlyCollection<CreateOrderItem> Items) : ICommand<long>;

public sealed record CreateOrderItem(string ProductName, decimal UnitPrice, int Quantity);
