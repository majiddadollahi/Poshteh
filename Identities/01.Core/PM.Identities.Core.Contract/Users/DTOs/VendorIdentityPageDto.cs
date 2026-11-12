namespace PM.Identities.Core.Contract.Users.DTOs;

public sealed record VendorIdentityPageDto(IReadOnlyList<VendorIdentityDto> Items, int TotalCount, int PageNumber, int PageSize);
