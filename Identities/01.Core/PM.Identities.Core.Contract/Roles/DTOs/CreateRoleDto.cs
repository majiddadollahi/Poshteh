using System.ComponentModel.DataAnnotations;

namespace PM.Identities.Core.Contract.Roles.DTOs;

public class CreateRoleDto
{
    [Required]
    [MinLength(2)]
    public string Name { get; set; } = string.Empty;
    
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;
}

