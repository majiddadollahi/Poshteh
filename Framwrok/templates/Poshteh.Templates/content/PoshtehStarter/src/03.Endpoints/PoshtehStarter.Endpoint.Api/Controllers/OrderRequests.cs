using PoshtehStarter.Core.ApplicationService.Ordering.Commands;
using PoshtehStarter.Core.Contract.Ordering;

namespace PoshtehStarter.Endpoint.Api.Controllers;

/// <summary>Body of create and update requests.</summary>
public sealed record OrderRequest(string CustomerName, IReadOnlyCollection<OrderItemRequest> Items)
{
    internal List<OrderItemInput> ToItems() =>
        Items.Select(item => new OrderItemInput(item.ProductName, item.UnitPrice, item.Quantity)).ToList();
}

public sealed record OrderItemRequest(string ProductName, decimal UnitPrice, int Quantity);

/// <summary>Query-string filters, sorting and paging for the order list.</summary>
public sealed class OrderSearchRequest
{
    /// <summary>Part of the customer name.</summary>
    public string? CustomerName { get; init; }

    /// <summary>Part of the name of any product in the order.</summary>
    public string? ProductName { get; init; }

    /// <summary>Created on or after this time (UTC).</summary>
    public DateTime? CreatedFrom { get; init; }

    /// <summary>Created on or before this time (UTC).</summary>
    public DateTime? CreatedTo { get; init; }

    public OrderSortBy SortBy { get; init; } = OrderSortBy.Id;

    public bool Descending { get; init; }

    /// <summary>1-based page number.</summary>
    public int Page { get; init; } = 1;

    /// <summary>Between 1 and 100.</summary>
    public int PageSize { get; init; } = 10;

    internal OrderSearchCriteria ToCriteria() =>
        new(CustomerName, ProductName, CreatedFrom, CreatedTo, SortBy, Descending, Page, PageSize);
}
