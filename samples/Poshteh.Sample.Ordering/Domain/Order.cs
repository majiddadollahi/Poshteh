using Poshteh.Core.Domain;
using Poshteh.Sample.Ordering.Domain.Events;

namespace Poshteh.Sample.Ordering.Domain;

/// <summary>
/// The consistency boundary for a customer order: owns its <see cref="OrderItem"/> line items and is the
/// only way they are created or changed. Every order must have a customer and at least one item — that
/// invariant is enforced here, never left to callers.
/// </summary>
public sealed class Order : AggregateRoot
{
    private readonly List<OrderItem> _items = [];

    private Order()
    {
    }

    private Order(string customerName)
    {
        CustomerName = customerName;
    }

    public string CustomerName { get; private set; } = string.Empty;

    public IReadOnlyList<OrderItem> Items => _items;

    public decimal TotalAmount => _items.Sum(item => item.LineTotal);

    public static Result<Order> Create(
        string customerName,
        IReadOnlyCollection<(string ProductName, decimal UnitPrice, int Quantity)> items)
    {
        if (string.IsNullOrWhiteSpace(customerName))
        {
            return Result.Failure<Order>(new Error("Order.CustomerNameRequired"));
        }

        if (items.Count == 0)
        {
            return Result.Failure<Order>(new Error("Order.RequiresAtLeastOneItem"));
        }

        var order = new Order(customerName.Trim());

        foreach (var item in items)
        {
            var addResult = order.AddItem(item.ProductName, item.UnitPrice, item.Quantity);
            if (addResult.IsFailure)
            {
                return Result.Failure<Order>(addResult.Error);
            }
        }

        order.AddDomainEvent(new OrderCreatedEvent(order.CustomerName, order._items.Count, order.TotalAmount));

        return order;
    }

    public Result AddItem(string productName, decimal unitPrice, int quantity)
    {
        if (string.IsNullOrWhiteSpace(productName))
        {
            return Result.Failure(new Error("Order.ProductNameRequired"));
        }

        if (unitPrice <= 0)
        {
            return Result.Failure(new Error("Order.UnitPriceMustBePositive"));
        }

        if (quantity <= 0)
        {
            return Result.Failure(new Error("Order.QuantityMustBePositive"));
        }

        _items.Add(new OrderItem(productName.Trim(), unitPrice, quantity));
        return Result.Success();
    }
}
