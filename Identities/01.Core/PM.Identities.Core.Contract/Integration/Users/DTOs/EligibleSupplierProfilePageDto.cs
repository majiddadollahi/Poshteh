namespace PM.Identities.Core.Contract.Integration.Users.DTOs;

public sealed record EligibleSupplierProfilePageDto(
    IReadOnlyList<EligibleSupplierProfileDto> Items,
    int TotalCount,
    int PageNumber,
    int PageSize);
