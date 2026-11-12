using Poshteh.Core.Domain;
using PM.Identities.Core.Domain.Features;

namespace PM.Identities.Core.Domain.Roles;

public class RoleFeature : BaseEntity
{
    public long RoleId { get; set; }
    public long FeatureId { get; set; }

    public Role Role { get; set; } = null!;
    public Feature Feature { get; set; } = null!;
}
