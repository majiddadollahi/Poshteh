using System.ComponentModel.DataAnnotations;

namespace PM.Identities.Core.Contract.Users.DTOs;

/// <summary>
/// Admin-panel password reset (no current password / email token required).
/// </summary>
public class AdminResetPasswordDto
{
    [Required]
    [MinLength(6)]
    public string Password { get; set; } = string.Empty;

    [Required]
    [Compare(nameof(Password))]
    public string ConfirmPassword { get; set; } = string.Empty;
}
