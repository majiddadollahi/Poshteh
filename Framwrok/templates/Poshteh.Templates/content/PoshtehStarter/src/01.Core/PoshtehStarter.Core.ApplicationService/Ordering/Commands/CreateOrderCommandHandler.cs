using Poshteh.Core.ApplicationService.Commands;
using Poshteh.Core.Domain;
using Poshteh.Shared.Kernel.Time;
using PoshtehStarter.Core.Contract.Ordering;
using PoshtehStarter.Core.Domain.Ordering;

namespace PoshtehStarter.Core.ApplicationService.Ordering.Commands;

/// <summary>
/// Invokes the domain factory that enforces the order's invariants, stages the new aggregate and commits.
/// The commit happens here rather than only in <c>TransactionBehavior</c> because the database-generated
/// <see cref="BaseEntity.Id"/> is needed as this command's result.
/// </summary>
public sealed class CreateOrderCommandHandler(IOrderCommandRepository orders, IUnitOfWork unitOfWork, IClock clock)
    : ICommandHandler<CreateOrderCommand, long>
{
    public async Task<Result<long>> Handle(CreateOrderCommand command, CancellationToken cancellationToken = default)
    {
        var orderResult = Order.Create(command.CustomerName, command.Items.ToTuples());
        if (orderResult.IsFailure)
        {
            return Result.Failure<long>(orderResult.Error);
        }

        var order = orderResult.Value;
        order.CreatedDate = clock.UtcNow.UtcDateTime;

        await orders.AddAsync(order, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        return order.Id;
    }
}
