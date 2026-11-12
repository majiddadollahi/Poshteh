namespace PM.Identities.Core.Contract.Users.DTOs;

public class AuthenticationResultDto
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public UserDto User { get; set; } = null!;

    /// <summary>
    /// فقط وقتی کاربر یک BuyerProfile (ماژول Procurement) دارد پر می‌شود؛ در غیر این صورت null است
    /// (کاربر خریدار نیست). این‌جا (سطح Endpoint/Host) پر می‌شود، نه داخل UserService — طبق تصمیم
    /// معماری BL-010: Core این ماژول به ماژول دیگری وابسته نمی‌شود.
    /// </summary>
    public BuyerCompanyInfoDto? BuyerCompany { get; set; }

    /// <summary>
    /// فقط وقتی کاربر یک Company (ماژول Vendor) دارد پر می‌شود؛ در غیر این صورت null است
    /// (کاربر تأمین‌کننده نیست).
    /// </summary>
    public SupplierCompanyInfoDto? SupplierCompany { get; set; }
}

