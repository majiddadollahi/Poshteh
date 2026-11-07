namespace PoshtehStarter.Core.Contract.Ordering;

/// <summary>
/// Filter, sort and paging input for <see cref="IOrderQueryRepository.SearchAsync"/>. Every filter is optional;
/// <see cref="Page"/> is 1-based.
/// </summary>
public sealed record OrderSearchCriteria(
    string? CustomerName,
    string? ProductName,
    DateTime? CreatedFrom,
    DateTime? CreatedTo,
    OrderSortBy SortBy,
    bool Descending,
    int Page,
    int PageSize);

public enum OrderSortBy
{
    Id,
    CustomerName,
    CreatedDate
}
