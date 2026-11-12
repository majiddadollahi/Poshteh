namespace PM.Identities.Core.Contract.Features.DTOs;

public sealed class FeatureLinkedAccessDto
{
    public long Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string PersianTitle { get; set; } = string.Empty;
    public string HttpMethod { get; set; } = string.Empty;
    public string RoutePattern { get; set; } = string.Empty;
    public string Application { get; set; } = string.Empty;
    public bool Enable { get; set; }
}
