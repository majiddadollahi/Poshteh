using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Poshteh.Infra.Data.Sql.Command.Outbox;

public class OutBoxEventItemConfiguration : IEntityTypeConfiguration<OutBoxEventItem>
{
    public void Configure(EntityTypeBuilder<OutBoxEventItem> builder)
    {
        builder.ToTable("OutboxEvents");

        builder.HasKey(x => x.EventId);
        builder.Property(x => x.EventId).ValueGeneratedNever();

        builder.Property(x => x.OccurredByUserId).HasMaxLength(128).IsRequired();
        builder.Property(x => x.AggregateId).HasMaxLength(128).IsRequired();
        builder.Property(x => x.AggregateName).HasMaxLength(256).IsRequired();
        builder.Property(x => x.AggregateTypeName).HasMaxLength(512).IsRequired();
        builder.Property(x => x.EventName).HasMaxLength(256).IsRequired();
        builder.Property(x => x.EventTypeName).HasMaxLength(512).IsRequired();
        builder.Property(x => x.EventPayload).IsRequired();
        builder.Property(x => x.TraceId).HasMaxLength(64);
        builder.Property(x => x.SpanId).HasMaxLength(64);
        builder.Property(x => x.LastError);

        builder.HasIndex(x => new { x.IsProcessed, x.OccurredOn });
    }
}
