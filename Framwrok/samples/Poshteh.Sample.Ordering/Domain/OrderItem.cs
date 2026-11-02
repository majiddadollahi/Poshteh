using Poshteh.Core.Domain;

namespace Poshteh.Sample.Ordering.Domain;

/// <summary>
/// A line item belonging to one <see cref="Ordering.Domain.Order"/>. Has identity (a plain Entity,
/// not an Aggregate Root) but no lifecycle of its own — it is only ever created, changed, and persisted
/// through its owning aggregate.
/// </summary>
public sealed class OrderItem : BaseEntity
{
    private OrderItem()
    {
    }

    internal OrderItem(string productName, decimal unitPrice, int quantity)
    {
        ProductName = productName;
        UnitPrice = unitPrice;
        Quantity = quantity;
    }

    public string ProductName { get; private set; } = string.Empty;

    public decimal UnitPrice { get; private set; }

    public int Quantity { get; private set; }

    public decimal LineTotal => UnitPrice * Quantity;
}
