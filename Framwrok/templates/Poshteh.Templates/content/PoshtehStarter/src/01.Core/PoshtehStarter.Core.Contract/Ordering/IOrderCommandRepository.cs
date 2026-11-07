using Poshteh.Core.Contract;
using PoshtehStarter.Core.Domain.Ordering;

namespace PoshtehStarter.Core.Contract.Ordering;

/// <summary>
/// Write-side port for <see cref="Order"/>. Extends the framework's get/add port with removal, which only
/// aggregates that can actually be deleted need; changes to loaded orders are saved by the Unit of Work commit.
/// </summary>
public interface IOrderCommandRepository : ICommandRepository<Order>
{
    void Remove(Order order);
}
