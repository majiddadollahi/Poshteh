using Poshteh.Core.Domain;

namespace PoshtehStarter.Core.Domain.Ordering.Events;

public sealed class OrderCreatedEvent(string customerName, int itemCount, decimal totalAmount) : DomainEvent
{
    public string CustomerName { get; } = customerName;

    public int ItemCount { get; } = itemCount;

    public decimal TotalAmount { get; } = totalAmount;
}
