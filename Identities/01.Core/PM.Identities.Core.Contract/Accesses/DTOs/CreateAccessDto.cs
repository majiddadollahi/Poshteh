using System.ComponentModel.DataAnnotations;

namespace PM.Identities.Core.Contract.Accesses.DTOs;

public class CreateAccessDto
{
    [Required]
    [MinLength(2)]
    public string Key { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    [RegularExpression("^RestEndpoint$")]
    public string Application { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string PersianTitle { get; set; } = string.Empty;

    [Required]
    [MaxLength(10)]
    [RegularExpression("(?i)^(GET|POST|PUT|PATCH|DELETE|HEAD|OPTIONS)$")]
    public string HttpMethod { get; set; } = string.Empty;

    [Required]
    [MaxLength(300)]
    public string RoutePattern { get; set; } = string.Empty;

    [Range(1, 4)]
    public byte AccessMode { get; set; } = 1;

    public bool Enable { get; set; } = true;
}
