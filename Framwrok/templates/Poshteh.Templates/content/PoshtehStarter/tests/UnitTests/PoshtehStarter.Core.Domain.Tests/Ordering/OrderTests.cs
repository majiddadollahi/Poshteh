using PoshtehStarter.Core.Domain.Ordering;
using PoshtehStarter.Core.Domain.Ordering.Events;

namespace PoshtehStarter.Core.Domain.Tests.Ordering;

public sealed class OrderTests
{
    [Fact]
    public void Create_WithValidInput_ReturnsOrderAndRaisesOrderCreatedEvent()
    {
        var result = Order.Create("  Ada Lovelace  ", [("Keyboard", 120.50m, 1), ("USB-C Hub", 35m, 2)]);

        Assert.True(result.IsSuccess);
        var order = result.Value;
        Assert.Equal("Ada Lovelace", order.CustomerName);
        Assert.Equal(2, order.Items.Count);
        Assert.Equal(190.50m, order.TotalAmount);

        var domainEvent = Assert.IsType<OrderCreatedEvent>(Assert.Single(order.Events()));
        Assert.Equal(2, domainEvent.ItemCount);
        Assert.Equal(190.50m, domainEvent.TotalAmount);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithoutCustomerName_Fails(string customerName)
    {
        var result = Order.Create(customerName, [("Keyboard", 1m, 1)]);

        Assert.True(result.IsFailure);
        Assert.Equal("Order.CustomerNameRequired", result.Error.Code);
    }

    [Fact]
    public void Create_WithoutItems_Fails()
    {
        var result = Order.Create("Ada Lovelace", []);

        Assert.True(result.IsFailure);
        Assert.Equal("Order.RequiresAtLeastOneItem", result.Error.Code);
    }

    [Fact]
    public void Update_WithValidInput_ReplacesCustomerAndItems()
    {
        var order = Order.Create("Ada Lovelace", [("Keyboard", 120.50m, 1), ("USB-C Hub", 35m, 2)]).Value;

        var result = order.Update("  Ada King  ", [("Monitor", 200m, 2)]);

        Assert.True(result.IsSuccess);
        Assert.Equal("Ada King", order.CustomerName);
        var item = Assert.Single(order.Items);
        Assert.Equal("Monitor", item.ProductName);
        Assert.Equal(400m, order.TotalAmount);
    }

    [Theory]
    [InlineData("", "Monitor", 1, 1, "Order.CustomerNameRequired")]
    [InlineData("Ada King", "", 1, 1, "Order.ProductNameRequired")]
    [InlineData("Ada King", "Monitor", 0, 1, "Order.UnitPriceMustBePositive")]
    [InlineData("Ada King", "Monitor", 1, 0, "Order.QuantityMustBePositive")]
    public void Update_WithInvalidInput_FailsAndLeavesOrderUnchanged(
        string customerName, string productName, decimal unitPrice, int quantity, string expectedCode)
    {
        var order = Order.Create("Ada Lovelace", [("Keyboard", 120.50m, 1)]).Value;

        var result = order.Update(customerName, [("Mouse", 10m, 1), (productName, unitPrice, quantity)]);

        Assert.True(result.IsFailure);
        Assert.Equal(expectedCode, result.Error.Code);
        Assert.Equal("Ada Lovelace", order.CustomerName);
        Assert.Equal("Keyboard", Assert.Single(order.Items).ProductName);
    }

    [Fact]
    public void Update_WithoutItems_Fails()
    {
        var order = Order.Create("Ada Lovelace", [("Keyboard", 120.50m, 1)]).Value;

        var result = order.Update("Ada King", []);

        Assert.True(result.IsFailure);
        Assert.Equal("Order.RequiresAtLeastOneItem", result.Error.Code);
    }

    [Theory]
    [InlineData("", 1, 1, "Order.ProductNameRequired")]
    [InlineData("Keyboard", 0, 1, "Order.UnitPriceMustBePositive")]
    [InlineData("Keyboard", 1, 0, "Order.QuantityMustBePositive")]
    public void Create_WithInvalidItem_Fails(string productName, decimal unitPrice, int quantity, string expectedCode)
    {
        var result = Order.Create("Ada Lovelace", [(productName, unitPrice, quantity)]);

        Assert.True(result.IsFailure);
        Assert.Equal(expectedCode, result.Error.Code);
    }
}
