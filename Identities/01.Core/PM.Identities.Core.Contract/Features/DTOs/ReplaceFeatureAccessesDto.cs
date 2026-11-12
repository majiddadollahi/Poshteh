namespace PM.Identities.Core.Contract.Features.DTOs;

public sealed class ReplaceFeatureAccessesDto
{
    public IReadOnlyCollection<long>? AccessIds { get; set; }
}
