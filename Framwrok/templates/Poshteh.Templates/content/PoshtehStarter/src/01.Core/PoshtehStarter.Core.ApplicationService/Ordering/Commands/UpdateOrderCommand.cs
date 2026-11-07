using Poshteh.Core.ApplicationService.Commands;

namespace PoshtehStarter.Core.ApplicationService.Ordering.Commands;

public sealed record UpdateOrderCommand(long OrderId, string CustomerName, IReadOnlyCollection<OrderItemInput> Items) : ICommand;
