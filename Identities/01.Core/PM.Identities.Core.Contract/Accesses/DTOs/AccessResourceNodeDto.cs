namespace PM.Identities.Core.Contract.Accesses.DTOs;

public sealed class AccessResourceNodeDto
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string NodeType { get; set; } = string.Empty;
    public long? AccessId { get; set; }
    public string? Key { get; set; }
    public string? HttpMethod { get; set; }
    public string? RoutePattern { get; set; }
    public string? Action { get; set; }
    public string? Module { get; set; }
    public string? Controller { get; set; }
    public bool? Enable { get; set; }
    public List<AccessResourceNodeDto> Children { get; set; } = [];
}
