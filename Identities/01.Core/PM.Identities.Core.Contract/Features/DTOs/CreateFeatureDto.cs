using System.ComponentModel.DataAnnotations;

namespace PM.Identities.Core.Contract.Features.DTOs;

public class CreateFeatureDto
{
    [Required]
    [MinLength(2)]
    public string Key { get; set; } = string.Empty;
    public int? NumericCode { get; set; }
    public string? PersianTitle { get; set; }
    public bool Enable { get; set; } = true;
    public long? ParentId { get; set; }
    public bool Dependent { get; set; }
}
