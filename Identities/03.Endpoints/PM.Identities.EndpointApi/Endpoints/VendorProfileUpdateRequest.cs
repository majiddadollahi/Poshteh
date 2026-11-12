using PM.Vendor.Core.Contract.Integration.Companies;
namespace PM.Identities.EndpointApi.Endpoints;
public sealed record VendorProfileUpdateRequest(string FirstName, string LastName, string? NationalCode,
    string? PhoneNumber, string? Address, VendorCompanyData? Company);
