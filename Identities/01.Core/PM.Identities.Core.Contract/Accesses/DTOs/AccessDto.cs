namespace PM.Identities.Core.Contract.Accesses.DTOs;

public class AccessDto
{
    public long Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Application { get; set; } = string.Empty;
    public string PersianTitle { get; set; } = string.Empty;
    public string HttpMethod { get; set; } = string.Empty;
    public string RoutePattern { get; set; } = string.Empty;
    public byte AccessMode { get; set; }
    public bool Enable { get; set; }
}
