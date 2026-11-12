namespace PM.Identities.Core.Contract.Accesses.DTOs;

public sealed class AccessListQuery
{
    public string? Search { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
