using Poshteh.Core.Domain;
using PM.Identities.Core.Domain.Features;

namespace PM.Identities.Core.Domain.Accesses;

/// <summary>
/// A protected application resource. This is the former Source concept.
/// </summary>
public class Access : AggregateRoot
{
    public string Key { get; set; } = string.Empty;
    public string Application { get; set; } = string.Empty;
    public string PersianTitle { get; set; } = string.Empty;
    public string HttpMethod { get; set; } = string.Empty;
    public string RoutePattern { get; set; } = string.Empty;
    public AccessMode AccessMode { get; set; }
    public AccessPanel Panel { get; set; }
    public AccessRequirements Requirements { get; set; }
    public bool Enable { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<FeatureAccess> FeatureAccesses { get; set; } = new List<FeatureAccess>();
}
