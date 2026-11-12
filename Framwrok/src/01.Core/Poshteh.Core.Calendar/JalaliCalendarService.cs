using System.Globalization;

namespace Poshteh.Core.Calendar;

/// <summary>
/// Formats Gregorian values as Jalali (Persian) strings for display only. Storage and business logic
/// stay Gregorian/UTC; these helpers never feed back into domain state.
/// </summary>
public static class JalaliCalendarService
{
    private static readonly PersianCalendar PersianCalendar = new();
    private static readonly Lazy<TimeZoneInfo> IranTimeZone = new(ResolveIranTimeZone);

    /// <summary>Formats a calendar date as <c>yyyy/MM/dd</c> without any time-zone conversion.</summary>
    public static string? FormatDate(DateOnly? date) =>
        date is { } value ? FormatDatePart(value.ToDateTime(TimeOnly.MinValue)) : null;

    /// <summary>Formats the date part (<c>yyyy/MM/dd</c>) of an instant after converting it to Iran time.</summary>
    public static string? FormatDate(DateTime? dateTime) =>
        dateTime is { } value ? FormatDatePart(ToIranTime(value)) : null;

    /// <summary>Formats an instant as <c>yyyy/MM/dd HH:mm</c> in Iran time.</summary>
    public static string? FormatDateTime(DateTime? dateTime)
    {
        if (dateTime is not { } value)
        {
            return null;
        }

        var local = ToIranTime(value);
        return FormatDatePart(local) + " " + local.ToString("HH:mm", CultureInfo.InvariantCulture);
    }

    /// <summary>Formats an instant as <c>yyyy/MM/dd HH:mm:ss</c> in Iran time.</summary>
    public static string? FormatDateTimeWithSeconds(DateTime? dateTime)
    {
        if (dateTime is not { } value)
        {
            return null;
        }

        var local = ToIranTime(value);
        return FormatDatePart(local) + " " + local.ToString("HH:mm:ss", CultureInfo.InvariantCulture);
    }

    private static string FormatDatePart(DateTime value) =>
        string.Create(
            CultureInfo.InvariantCulture,
            $"{PersianCalendar.GetYear(value):D4}/{PersianCalendar.GetMonth(value):D2}/{PersianCalendar.GetDayOfMonth(value):D2}");

    // Unspecified is deliberately treated as UTC: persisted timestamps come back from the database
    // without a Kind, and they are stored as UTC.
    private static DateTime ToIranTime(DateTime value)
    {
        var utc = value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
        };

        return TimeZoneInfo.ConvertTimeFromUtc(utc, IranTimeZone.Value);
    }

    private static TimeZoneInfo ResolveIranTimeZone()
    {
        var id = OperatingSystem.IsWindows() ? "Iran Standard Time" : "Asia/Tehran";
        if (TimeZoneInfo.TryFindSystemTimeZoneById(id, out var timeZone))
        {
            return timeZone;
        }

        return TimeZoneInfo.CreateCustomTimeZone(
            "Iran Standard Time",
            new TimeSpan(3, 30, 0),
            "Iran Standard Time",
            "Iran Standard Time");
    }
}
