namespace PM.Identities.Core.Contract.Accesses.DTOs;

public class MyAdminAccessesDto
{
    public bool IsAdmin { get; set; }
    public bool CanEnterAdminPanel { get; set; }
    public List<string> Keys { get; set; } = [];
    public List<string> ManagedOrganizationCodes { get; set; } = [];

    /// <summary>
    /// Monotonic version for this user. UI should refresh when the value increases.
    /// </summary>
    public long AccessVersion { get; set; }
}
