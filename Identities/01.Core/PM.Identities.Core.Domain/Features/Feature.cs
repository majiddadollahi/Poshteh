using Poshteh.Core.Domain;
using PM.Identities.Core.Domain.Roles;

namespace PM.Identities.Core.Domain.Features;

/// <summary>
/// A grantable capability assigned to roles.
/// </summary>
public class Feature : AggregateRoot
{
    public string Key { get; set; } = string.Empty;
    public int? NumericCode { get; set; }
    public string? PersianTitle { get; set; }
    public bool Enable { get; set; } = true;
    public long? ParentId { get; set; }
    public Feature? Parent { get; set; }
    public ICollection<Feature> Children { get; set; } = new List<Feature>();
    public bool Dependent { get; set; }

    public ICollection<RoleFeature> RoleFeatures { get; set; } = new List<RoleFeature>();
    public ICollection<FeatureAccess> FeatureAccesses { get; set; } = new List<FeatureAccess>();
}
