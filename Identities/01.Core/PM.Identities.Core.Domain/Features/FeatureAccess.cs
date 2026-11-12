using PM.Identities.Core.Domain.Accesses;

namespace PM.Identities.Core.Domain.Features;

public class FeatureAccess
{
    public long FeatureId { get; set; }
    public long AccessId { get; set; }
    public DateTime CreatedAt { get; set; }

    public Feature Feature { get; set; } = null!;
    public Access Access { get; set; } = null!;
}
