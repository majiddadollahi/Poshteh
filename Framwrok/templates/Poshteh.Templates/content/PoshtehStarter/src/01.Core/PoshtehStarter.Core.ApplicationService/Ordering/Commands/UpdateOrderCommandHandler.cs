using Poshteh.Core.ApplicationService;
using Poshteh.Core.ApplicationService.Commands;
using Poshteh.Core.Domain;
using Poshteh.Shared.Kernel.Time;
using PoshtehStarter.Core.Contract.Ordering;

namespace PoshtehStarter.Core.ApplicationService.Ordering.Commands;

/// <summary>
/// Loads the aggregate, lets it apply and validate the change, and leaves the commit to
/// <c>TransactionBehavior</c>: the loaded order is tracked, so the Unit of Work saves it.
/// </summary>
public sealed class UpdateOrderCommandHandler(IOrderCommandRepository orders, IClock clock)
    : ICommandHandler<UpdateOrderCommand>
{
    public async Task<Result<Unit>> Handle(UpdateOrderCommand command, CancellationToken cancellationToken = default)
    {
        var order = await orders.GetByIdAsync(command.OrderId, cancellationToken);
        if (order is null)
        {
            return Result.Failure<Unit>(OrderErrors.NotFound);
        }

        var updateResult = order.Update(command.CustomerName, command.Items.ToTuples());
        if (updateResult.IsFailure)
        {
            return Result.Failure<Unit>(updateResult.Error);
        }

        order.UpdateDate = clock.UtcNow.UtcDateTime;

        return Unit.Value;
    }
}
