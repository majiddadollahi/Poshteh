namespace PM.Identities.Core.Contract.Users.DTOs;

public class UserListFilterDto
{
    /// <summary>
    /// جستجو در نام، نام کاربری، ایمیل و شماره تلفن
    /// </summary>
    public string? Search { get; set; }
    public string? UserName { get; set; }
    public string? Email { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? PhoneNumber { get; set; }
    public bool? IsActive { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

