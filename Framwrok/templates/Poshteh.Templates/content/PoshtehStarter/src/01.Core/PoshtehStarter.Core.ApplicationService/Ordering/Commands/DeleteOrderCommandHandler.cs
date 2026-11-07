using Poshteh.Core.ApplicationService;
using Poshteh.Core.ApplicationService.Commands;
using Poshteh.Core.Domain;
using PoshtehStarter.Core.Contract.Ordering;

namespace PoshtehStarter.Core.ApplicationService.Ordering.Commands;

/// <summary>
/// Removes the order together with its line items; <c>TransactionBehavior</c> commits the removal.
/// </summary>
public sealed class DeleteOrderCommandHandler(IOrderCommandRepository orders) : ICommandHandler<DeleteOrderCommand>
{
    public async Task<Result<Unit>> Handle(DeleteOrderCommand command, CancellationToken cancellationToken = default)
    {
        var order = await orders.GetByIdAsync(command.OrderId, cancellationToken);
        if (order is null)
        {
            return Result.Failure<Unit>(OrderErrors.NotFound);
        }

        orders.Remove(order);

        return Unit.Value;
    }
}
