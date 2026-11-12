namespace Poshteh.Core.Calendar;

/// <summary>
/// Naming convention for read-model display fields that sit next to a Gregorian property,
/// e.g. <c>CreatedAt</c> → <c>CreatedAtJalali</c>.
/// </summary>
public static class JalaliDisplayFields
{
    public const string Suffix = "Jalali";

    public static string PropertyNameFor(string gregorianPropertyName)
    {
        ArgumentNullException.ThrowIfNull(gregorianPropertyName);
        return gregorianPropertyName + Suffix;
    }
}
