namespace PM.Identities.Core.Contract.Features.DTOs;

public class FeatureDto
{
    public long Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public int? NumericCode { get; set; }
    public string? PersianTitle { get; set; }
    public bool Enable { get; set; }
    public long? ParentId { get; set; }
    public bool Dependent { get; set; }
}
