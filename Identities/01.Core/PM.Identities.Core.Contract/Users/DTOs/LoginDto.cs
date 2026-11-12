using System.ComponentModel.DataAnnotations;
namespace PM.Identities.Core.Contract.Users.DTOs;
public class LoginDto : IValidatableObject
{
    public string? Identifier { get; set; }
    // Backward compatibility for existing Buyer/Admin/mobile clients.
    public string? UserName { get; set; }
    [Required] public string Password { get; set; } = "";
    public string EffectiveIdentifier => string.IsNullOrWhiteSpace(Identifier) ? UserName ?? "" : Identifier;
    public IEnumerable<ValidationResult> Validate(ValidationContext context)
    {
        if (string.IsNullOrWhiteSpace(EffectiveIdentifier)) yield return new ValidationResult("شناسه ورود الزامی است.", [nameof(Identifier)]);
    }
}
