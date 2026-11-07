using Poshteh.Core.ApplicationService.Commands;

namespace PoshtehStarter.Core.ApplicationService.Ordering.Commands;

public sealed record DeleteOrderCommand(long OrderId) : ICommand;
