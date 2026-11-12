namespace PM.Identities.Core.Domain.Accesses;

public enum AccessMode : byte
{
    AdminFeatureGated = 1,
    SelfServiceAuthenticated = 2,
    SharedReadAuthenticated = 3,
    Public = 4
}
