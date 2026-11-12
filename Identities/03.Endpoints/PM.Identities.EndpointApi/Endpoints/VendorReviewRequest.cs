using System.ComponentModel.DataAnnotations;
using PM.Identities.Core.Domain.Users;

namespace PM.Identities.EndpointApi.Endpoints;

public sealed record VendorReviewRequest(
    VendorReviewStatus ExpectedStatus,
    [MaxLength(1000)] string? RejectionReason = null);
