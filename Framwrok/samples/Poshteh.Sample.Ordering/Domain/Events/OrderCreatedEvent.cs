using Poshteh.Core.Domain;

namespace Poshteh.Sample.Ordering.Domain.Events;

public sealed class OrderCreatedEvent(string customerName, int itemCount, decimal totalAmount) : DomainEvent
{
    public string CustomerName { get; } = customerName;

    public int ItemCount { get; } = itemCount;

    public decimal TotalAmount { get; } = totalAmount;
}
