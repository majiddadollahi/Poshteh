using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PM.Identities.Core.Domain.Accesses;

namespace PM.Identities.Infra.DAL.Configurations;

public sealed class AccessConfiguration : IEntityTypeConfiguration<Access>
{
    public void Configure(EntityTypeBuilder<Access> builder)
    {
        // Access uses the resource-specific CreatedAt/UpdatedAt timestamps. The
        // inherited audit members were never part of the former Sources table.
        builder.Ignore(access => access.CreatedBy);
        builder.Ignore(access => access.CreatedDate);
        builder.Ignore(access => access.ModifiedBy);
        builder.Ignore(access => access.UpdateDate);

        builder.ToTable("Accesses", table =>
        {
            table.HasCheckConstraint("CK_Accesses_AccessMode", "[AccessMode] BETWEEN 1 AND 4");
            table.HasCheckConstraint("CK_Accesses_Panel", "[Panel] BETWEEN 0 AND 6");
            table.HasCheckConstraint("CK_Accesses_Requirements", "[Requirements] BETWEEN 0 AND 3");
            table.HasCheckConstraint("CK_Accesses_Application", "[Application] = 'RestEndpoint'");
            table.HasCheckConstraint("CK_Accesses_Key_NotBlank", "LEN(LTRIM(RTRIM([Key]))) > 0");
            table.HasCheckConstraint("CK_Accesses_PersianTitle_NotBlank", "LEN(LTRIM(RTRIM([PersianTitle]))) > 0");
            table.HasCheckConstraint("CK_Accesses_RoutePattern_NotBlank", "LEN(LTRIM(RTRIM([RoutePattern]))) > 0");
            table.HasCheckConstraint(
                "CK_Accesses_HttpMethod",
                "[HttpMethod] IN ('GET','POST','PUT','PATCH','DELETE','HEAD','OPTIONS')");
        });

        builder.Property(access => access.Key).HasMaxLength(150).IsRequired();
        builder.Property(access => access.Application).HasMaxLength(50).IsRequired();
        builder.Property(access => access.PersianTitle).HasMaxLength(200).IsRequired();
        builder.Property(access => access.HttpMethod).HasMaxLength(10).IsRequired();
        builder.Property(access => access.RoutePattern).HasMaxLength(300).IsRequired();
        builder.Property(access => access.AccessMode).HasConversion<byte>().HasColumnType("tinyint").IsRequired();
        // TASK-0247: Panel=None (0) is the safe default for rows the catalog sync has not filled yet.
        builder.Property(access => access.Panel).HasConversion<byte>().HasColumnType("tinyint").HasDefaultValue(AccessPanel.None).IsRequired();
        builder.Property(access => access.Requirements).HasConversion<byte>().HasColumnType("tinyint").HasDefaultValue(AccessRequirements.None).IsRequired();
        builder.Property(access => access.Enable).HasDefaultValue(true).IsRequired();
        builder.Property(access => access.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()").IsRequired();
        builder.Property(access => access.UpdatedAt).HasDefaultValueSql("SYSUTCDATETIME()").IsRequired();

        builder.HasIndex(access => access.Key)
            .IsUnique()
            .HasDatabaseName("UX_Accesses_Key");

        builder.HasIndex(access => new { access.Application, access.HttpMethod, access.RoutePattern })
            .IsUnique()
            .HasDatabaseName("UX_Accesses_ApiIdentity");
    }
}
