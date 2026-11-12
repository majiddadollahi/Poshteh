namespace PM.Identities.Infra.DAL.Bootstrap;

public sealed class BootstrapAdminOptions
{
    public const string SectionName = "BootstrapAdmin";

    public bool Enabled { get; set; }
    public string UserName { get; set; } = "dadollahi";
    public string FirstName { get; set; } = "مجید";
    public string LastName { get; set; } = "داداللهی";
    public string? Password { get; set; }
}
