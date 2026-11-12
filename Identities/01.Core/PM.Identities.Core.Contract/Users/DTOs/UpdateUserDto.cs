using System.ComponentModel.DataAnnotations;

namespace PM.Identities.Core.Contract.Users.DTOs;

public class UpdateUserDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    public string FirstName { get; set; } = string.Empty;
    
    [Required]
    public string LastName { get; set; } = string.Empty;
    
    public string? PhoneNumber { get; set; }
    public string? NationalId { get; set; }
    public string? ProfilePictureUrl { get; set; }
}

