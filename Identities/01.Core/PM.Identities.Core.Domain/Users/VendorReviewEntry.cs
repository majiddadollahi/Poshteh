namespace PM.Identities.Core.Domain.Users;
public sealed class VendorReviewEntry
{
    public long Id { get; set; }
    public long VendorUserId { get; set; }
    public VendorReviewStatus FromStatus { get; set; }
    public VendorReviewStatus ToStatus { get; set; }
    public DateTime OccurredAt { get; set; }
    public long ActorUserId { get; set; }
    public string? Reason { get; set; }
}
