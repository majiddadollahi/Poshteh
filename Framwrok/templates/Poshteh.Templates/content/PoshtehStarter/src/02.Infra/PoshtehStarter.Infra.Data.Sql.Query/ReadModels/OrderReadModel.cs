namespace PoshtehStarter.Infra.Data.Sql.Query.ReadModels;

/// <summary>
/// Persistence-shaped view of the <c>Orders</c> table for the read side. Never used to change data.
/// </summary>
public sealed class OrderReadModel
{
    public long Id { get; set; }

    public string CustomerName { get; set; } = string.Empty;

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdateDate { get; set; }

    public List<OrderItemReadModel> Items { get; set; } = [];
}

public sealed class OrderItemReadModel
{
    public long Id { get; set; }

    public long OrderId { get; set; }

    public string ProductName { get; set; } = string.Empty;

    public decimal UnitPrice { get; set; }

    public int Quantity { get; set; }
}
