namespace Poshteh.Infra.Data.Sql.Command.Outbox;

/// <summary>
/// A row written in the same transaction as the aggregate change that raised it
/// (the Transactional Outbox pattern). Dispatched later, out-of-band, by a background worker.
/// </summary>
public class OutBoxEventItem
{
    public Guid EventId { get; set; }
    public string OccurredByUserId { get; set; } = string.Empty;
    public DateTime OccurredOn { get; set; }
    public string AggregateId { get; set; } = string.Empty;
    public string AggregateName { get; set; } = string.Empty;
    public string AggregateTypeName { get; set; } = string.Empty;
    public string EventName { get; set; } = string.Empty;
    public string EventTypeName { get; set; } = string.Empty;
    public string EventPayload { get; set; } = string.Empty;
    public string TraceId { get; set; } = string.Empty;
    public string SpanId { get; set; } = string.Empty;
    public bool IsProcessed { get; set; }
    public DateTime? ProcessedOn { get; set; }
    public int RetryCount { get; set; }
    public string? LastError { get; set; }
}
