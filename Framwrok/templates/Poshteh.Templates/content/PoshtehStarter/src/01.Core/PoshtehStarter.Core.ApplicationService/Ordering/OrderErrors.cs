using Poshteh.Core.Domain;

namespace PoshtehStarter.Core.ApplicationService.Ordering;

/// <summary>
/// Application-level errors for the ordering use cases. The code decides the HTTP status at the API boundary:
/// "NotFound" maps to 404 and "Validation" to 422.
/// </summary>
public static class OrderErrors
{
    public static readonly Error NotFound = new("Order.NotFound", "The order was not found.");

    public static readonly Error InvalidPage = new("Order.Validation.InvalidPage", "Page must be 1 or greater.");

    public static readonly Error InvalidPageSize = new("Order.Validation.InvalidPageSize", "PageSize must be between 1 and 100.");

    public static readonly Error InvalidDateRange = new("Order.Validation.InvalidDateRange", "CreatedFrom must not be after CreatedTo.");
}
