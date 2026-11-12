namespace PM.Identities.Core.ApplicationService.Features;

public sealed record AdminFeatureCatalogSeedResult(
    int ManagedDefinitions,
    int Inserted,
    int Updated,
    int UnmanagedPreserved,
    int UnmanagedRoleGrantsPreserved);
