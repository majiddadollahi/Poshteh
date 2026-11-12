using System.ComponentModel.DataAnnotations;
using PM.Identities.Core.Domain.Users;

namespace PM.Identities.Core.Contract.Users.DTOs;

public class CreateUserDto
{
    [Required]
    [MinLength(3)]
    public string UserName { get; set; } = string.Empty;
    
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    [MinLength(6)]
    public string Password { get; set; } = string.Empty;

    [Required]
    [MinLength(6)]
    [Compare(nameof(Password))]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Required]
    public string FirstName { get; set; } = string.Empty;
    
    [Required]
    public string LastName { get; set; } = string.Empty;
    
    public string? PhoneNumber { get; set; }
    public string? NationalId { get; set; }

    [Required]
    public AccountType? AccountType { get; set; }
}
