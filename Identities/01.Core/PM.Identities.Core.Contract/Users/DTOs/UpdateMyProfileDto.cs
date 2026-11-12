using System.ComponentModel.DataAnnotations;

namespace PM.Identities.Core.Contract.Users.DTOs;

public sealed class UpdateMyProfileDto
{
    [Required, MaxLength(50)] public string FirstName { get; set; } = string.Empty;
    [Required, MaxLength(50)] public string LastName { get; set; } = string.Empty;
    [RegularExpression(@"^[0۰٠][0-9۰-۹٠-٩]{4,14}$")] public string? PhoneNumber { get; set; }
}
