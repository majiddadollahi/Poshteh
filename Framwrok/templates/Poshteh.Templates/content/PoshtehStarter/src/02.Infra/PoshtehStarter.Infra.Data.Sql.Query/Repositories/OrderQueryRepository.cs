using Microsoft.EntityFrameworkCore;
using PoshtehStarter.Core.Contract.Common;
using PoshtehStarter.Core.Contract.Ordering;
using PoshtehStarter.Infra.Data.Sql.Query.DBContexts;
using PoshtehStarter.Infra.Data.Sql.Query.ReadModels;

namespace PoshtehStarter.Infra.Data.Sql.Query.Repositories;

public sealed class OrderQueryRepository(OrderingQueryDbContext dbContext) : IOrderQueryRepository
{
    public async Task<OrderDto?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var order = await dbContext.Orders
            .Include(o => o.Items)
            .SingleOrDefaultAsync(o => o.Id == id, cancellationToken);

        if (order is null)
        {
            return null;
        }

        var items = order.Items
            .OrderBy(item => item.Id)
            .Select(item => new OrderItemDto(item.Id, item.ProductName, item.UnitPrice, item.Quantity, item.UnitPrice * item.Quantity))
            .ToList();

        return new OrderDto(order.Id, order.CustomerName, order.CreatedDate, order.UpdateDate, items.Sum(item => item.LineTotal), items);
    }

    public async Task<PagedResult<OrderSummaryDto>> SearchAsync(OrderSearchCriteria criteria, CancellationToken cancellationToken = default)
    {
        var query = ApplyFilters(dbContext.Orders, criteria);

        var totalCount = await query.CountAsync(cancellationToken);

        var page = await ApplySorting(query, criteria)
            .Skip((criteria.Page - 1) * criteria.PageSize)
            .Take(criteria.PageSize)
            .Select(o => new
            {
                o.Id,
                o.CustomerName,
                o.CreatedDate,
                Lines = o.Items.Select(i => new { i.UnitPrice, i.Quantity }).ToList()
            })
            .ToListAsync(cancellationToken);

        // Totals are summed in memory: some providers (e.g. SQLite) cannot aggregate decimal columns in SQL.
        var items = page
            .Select(o => new OrderSummaryDto(
                o.Id,
                o.CustomerName,
                o.CreatedDate,
                o.Lines.Count,
                o.Lines.Sum(line => line.UnitPrice * line.Quantity)))
            .ToList();

        return new PagedResult<OrderSummaryDto>(items, criteria.Page, criteria.PageSize, totalCount);
    }

    private static IQueryable<OrderReadModel> ApplyFilters(IQueryable<OrderReadModel> query, OrderSearchCriteria criteria)
    {
        if (!string.IsNullOrWhiteSpace(criteria.CustomerName))
        {
            var customerName = criteria.CustomerName.Trim();
            query = query.Where(o => o.CustomerName.Contains(customerName));
        }

        if (!string.IsNullOrWhiteSpace(criteria.ProductName))
        {
            var productName = criteria.ProductName.Trim();
            query = query.Where(o => o.Items.Any(i => i.ProductName.Contains(productName)));
        }

        if (criteria.CreatedFrom is { } createdFrom)
        {
            query = query.Where(o => o.CreatedDate >= createdFrom);
        }

        if (criteria.CreatedTo is { } createdTo)
        {
            query = query.Where(o => o.CreatedDate <= createdTo);
        }

        return query;
    }

    private static IQueryable<OrderReadModel> ApplySorting(IQueryable<OrderReadModel> query, OrderSearchCriteria criteria) =>
        (criteria.SortBy, criteria.Descending) switch
        {
            (OrderSortBy.CustomerName, false) => query.OrderBy(o => o.CustomerName).ThenBy(o => o.Id),
            (OrderSortBy.CustomerName, true) => query.OrderByDescending(o => o.CustomerName).ThenByDescending(o => o.Id),
            (OrderSortBy.CreatedDate, false) => query.OrderBy(o => o.CreatedDate).ThenBy(o => o.Id),
            (OrderSortBy.CreatedDate, true) => query.OrderByDescending(o => o.CreatedDate).ThenByDescending(o => o.Id),
            (_, false) => query.OrderBy(o => o.Id),
            (_, true) => query.OrderByDescending(o => o.Id)
        };
}
