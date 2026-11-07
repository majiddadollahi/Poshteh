using Poshteh.Core.ApplicationService.Commands;

namespace PoshtehStarter.Core.ApplicationService.Ordering.Commands;

public sealed record CreateOrderCommand(string CustomerName, IReadOnlyCollection<OrderItemInput> Items) : ICommand<long>;

public sealed record OrderItemInput(string ProductName, decimal UnitPrice, int Quantity);
