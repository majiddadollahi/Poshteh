using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PM.Identities.Core.Domain.Features;

namespace PM.Identities.Infra.DAL.Configurations;

public sealed class FeatureAccessConfiguration : IEntityTypeConfiguration<FeatureAccess>
{
    public void Configure(EntityTypeBuilder<FeatureAccess> builder)
    {
        builder.ToTable("FeatureAccesses");
        builder.HasKey(link => new { link.FeatureId, link.AccessId });
        builder.Property(link => link.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()").IsRequired();

        builder.HasOne(link => link.Feature)
            .WithMany(feature => feature.FeatureAccesses)
            .HasForeignKey(link => link.FeatureId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(link => link.Access)
            .WithMany(access => access.FeatureAccesses)
            .HasForeignKey(link => link.AccessId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(link => link.AccessId);
    }
}
