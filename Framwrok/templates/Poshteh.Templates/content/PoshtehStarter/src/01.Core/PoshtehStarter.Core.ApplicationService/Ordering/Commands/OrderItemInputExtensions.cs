namespace PoshtehStarter.Core.ApplicationService.Ordering.Commands;

internal static class OrderItemInputExtensions
{
    public static List<(string ProductName, decimal UnitPrice, int Quantity)> ToTuples(this IEnumerable<OrderItemInput> items) =>
        items.Select(item => (item.ProductName, item.UnitPrice, item.Quantity)).ToList();
}
