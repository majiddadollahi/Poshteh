using Poshteh.Core.Domain;
using PoshtehStarter.Core.Domain.Ordering.Events;

namespace PoshtehStarter.Core.Domain.Ordering;

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
        var validation = Validate(customerName, items);
        if (validation.IsFailure)
        {
            return Result.Failure<Order>(validation.Error);
        }

        var order = new Order(customerName.Trim());

        foreach (var item in items)
        {
            order._items.Add(new OrderItem(item.ProductName.Trim(), item.UnitPrice, item.Quantity));
        }

        order.AddDomainEvent(new OrderCreatedEvent(order.CustomerName, order._items.Count, order.TotalAmount));

        return order;
    }

    /// <summary>
    /// Replaces the customer and the whole set of line items. All input is validated before anything changes,
    /// so a failed update leaves the order exactly as it was.
    /// </summary>
    public Result Update(
        string customerName,
        IReadOnlyCollection<(string ProductName, decimal UnitPrice, int Quantity)> items)
    {
        var validation = Validate(customerName, items);
        if (validation.IsFailure)
        {
            return validation;
        }

        CustomerName = customerName.Trim();

        _items.Clear();
        foreach (var item in items)
        {
            _items.Add(new OrderItem(item.ProductName.Trim(), item.UnitPrice, item.Quantity));
        }

        return Result.Success();
    }

    public Result AddItem(string productName, decimal unitPrice, int quantity)
    {
        var validation = ValidateItem(productName, unitPrice, quantity);
        if (validation.IsFailure)
        {
            return validation;
        }

        _items.Add(new OrderItem(productName.Trim(), unitPrice, quantity));
        return Result.Success();
    }

    private static Result Validate(
        string customerName,
        IReadOnlyCollection<(string ProductName, decimal UnitPrice, int Quantity)> items)
    {
        if (string.IsNullOrWhiteSpace(customerName))
        {
            return Result.Failure(new Error("Order.CustomerNameRequired", "Customer name is required."));
        }

        if (items.Count == 0)
        {
            return Result.Failure(new Error("Order.RequiresAtLeastOneItem", "An order must have at least one item."));
        }

        foreach (var item in items)
        {
            var itemValidation = ValidateItem(item.ProductName, item.UnitPrice, item.Quantity);
            if (itemValidation.IsFailure)
            {
                return itemValidation;
            }
        }

        return Result.Success();
    }

    private static Result ValidateItem(string productName, decimal unitPrice, int quantity)
    {
        if (string.IsNullOrWhiteSpace(productName))
        {
            return Result.Failure(new Error("Order.ProductNameRequired", "Product name is required."));
        }

        if (unitPrice <= 0)
        {
            return Result.Failure(new Error("Order.UnitPriceMustBePositive", "Unit price must be greater than zero."));
        }

        if (quantity <= 0)
        {
            return Result.Failure(new Error("Order.QuantityMustBePositive", "Quantity must be greater than zero."));
        }

        return Result.Success();
    }
}
