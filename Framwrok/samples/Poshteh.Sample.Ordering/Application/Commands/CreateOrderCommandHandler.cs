using Poshteh.Core.ApplicationService.Commands;
using Poshteh.Core.Contract;
using Poshteh.Core.Domain;
using Poshteh.Sample.Ordering.Domain;

namespace Poshteh.Sample.Ordering.Application.Commands;

/// <summary>
/// Loads no aggregate (this use case creates one), invokes the domain factory that enforces
/// the order's invariants, then persists it and commits — the standard write-side shape:
/// <see cref="ICommandRepository{TEntity}"/> to stage the change, <see cref="IUnitOfWork"/> to commit it.
/// </summary>
public sealed class CreateOrderCommandHandler(ICommandRepository<Order> orders, IUnitOfWork unitOfWork)
    : ICommandHandler<CreateOrderCommand, long>
{
    public async Task<Result<long>> Handle(CreateOrderCommand command, CancellationToken cancellationToken = default)
    {
        var items = command.Items
            .Select(item => (item.ProductName, item.UnitPrice, item.Quantity))
            .ToList();

        var orderResult = Order.Create(command.CustomerName, items);
        if (orderResult.IsFailure)
        {
            return Result.Failure<long>(orderResult.Error);
        }

        var order = orderResult.Value;

        await orders.AddAsync(order, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        return order.Id;
    }
}
