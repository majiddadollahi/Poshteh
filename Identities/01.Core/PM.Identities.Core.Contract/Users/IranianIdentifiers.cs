namespace PM.Identities.Core.Contract.Users;
/// <summary>Reusable syntax/check-digit rules; these do not prove ownership of an identifier.</summary>
public static class IranianIdentifiers
{
    public static string Normalize(string? value) => new((value ?? "").Trim().Select(c =>
        c is >= '۰' and <= '۹' ? (char)('0' + c - '۰') :
        c is >= '٠' and <= '٩' ? (char)('0' + c - '٠') : c).ToArray());
    private static bool Digits(string s, int length) => s.Length == length && s.All(c => c is >= '0' and <= '9') && s.Distinct().Count() > 1;
    public static bool IsPersonal(string? value)
    {
        var s = Normalize(value);
        if (!Digits(s, 10)) return false;
        var remainder = Enumerable.Range(0, 9).Sum(i => (s[i] - '0') * (10 - i)) % 11;
        return s[9] - '0' == (remainder < 2 ? remainder : 11 - remainder);
    }
    public static bool IsLegal(string? value)
    {
        var s = Normalize(value);
        if (!Digits(s, 11)) return false;
        if (s.Substring(3, 6) == "000000") return false; // positions 4-9 all zero — structurally invalid even if the checksum happens to pass
        int[] weights = [29, 27, 23, 19, 17];
        var offset = s[9] - '0' + 2;
        var remainder = Enumerable.Range(0, 10).Sum(i => (s[i] - '0' + offset) * weights[i % 5]) % 11;
        return s[10] - '0' == (remainder == 10 ? 0 : remainder);
    }
}
