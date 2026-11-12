using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PM.Identities.Core.Domain.Features;

namespace PM.Identities.Infra.DAL.Configurations;

public sealed class FeatureConfiguration : IEntityTypeConfiguration<Feature>
{
    public void Configure(EntityTypeBuilder<Feature> builder)
    {
        builder.ToTable("Features");
        builder.Property(feature => feature.Key).HasMaxLength(450).IsRequired();

        builder.HasOne(feature => feature.Parent)
            .WithMany(feature => feature.Children)
            .HasForeignKey(feature => feature.ParentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(feature => feature.Key)
            .IsUnique()
            .HasDatabaseName("UX_Features_Key");

        builder.HasIndex(feature => feature.NumericCode)
            .IsUnique()
            .HasFilter("[NumericCode] IS NOT NULL")
            .HasDatabaseName("UX_Features_NumericCode");
    }
}
