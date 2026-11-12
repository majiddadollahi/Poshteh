namespace PM.Identities.EndpointApi.Endpoints;

public sealed record VendorPageDto(IReadOnlyList<VendorProfileDto> Items, int TotalCount, int PageNumber, int PageSize);
