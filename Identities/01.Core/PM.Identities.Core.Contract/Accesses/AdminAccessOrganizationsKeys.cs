namespace PM.Identities.Core.Contract.Accesses;

public static class AdminAccessOrganizationsKeys
{
    public const string Node = "Admin.Organizations";
    public const string CatalogCreate = "Admin.Organizations.Catalog.Create";

    public static string ForTenant(string tenantCode) =>
        $"{Node}.{AccessKeyNormalizer.Normalize(tenantCode)}";
}
