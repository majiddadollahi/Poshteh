using System.ComponentModel.DataAnnotations;

namespace PM.Identities.Core.Contract.Roles.DTOs;

public class UpdateRoleDto
{
    [Required]
    [MinLength(2)]
    public string Name { get; set; } = string.Empty;
    
    public string? Description { get; set; }
}

