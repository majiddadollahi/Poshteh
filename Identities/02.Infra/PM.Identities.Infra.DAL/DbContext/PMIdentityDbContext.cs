using PM.Identities.Core.Domain.Accesses;
using PM.Identities.Core.Domain.Features;
using PM.Identities.Core.Domain.Roles;
using PM.Identities.Core.Domain.Users;
using PM.Identities.Infra.DAL.Constants;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace PM.Identities.Infra.DAL.DbContext;

/// <summary>
/// DbContext برای ماژول Identity
/// </summary>
public class PMIdentityDbContext : IdentityDbContext<User, Role, long,
    UserClaim, UserRole, UserLogin, RoleClaim, UserToken>
{
    public PMIdentityDbContext(DbContextOptions<PMIdentityDbContext> options)
        : base(options)
    {
    }

    public DbSet<Access> Accesses { get; set; }
    public DbSet<Feature> Features { get; set; }
    public DbSet<RoleFeature> RoleFeatures { get; set; }
    public DbSet<FeatureAccess> FeatureAccesses { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<UserSession> UserSessions { get; set; }
    public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }
    public DbSet<EmailVerificationToken> EmailVerificationTokens { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.HasDefaultSchema(IdentitySchema.Name);
        builder.Entity<User>().Property(u => u.AccountType).HasConversion<int>().IsRequired();
        builder.Entity<User>().ToTable(table => table.HasCheckConstraint(
            "CK_AspNetUsers_AccountType", "[AccountType] BETWEEN 1 AND 9"));
        builder.Entity<User>().Property(u => u.VendorRejectionReason).HasMaxLength(1000);
        builder.Entity<User>().Property(u => u.VendorCorrectionReason).HasMaxLength(1000);
        builder.Entity<User>().HasIndex(u => u.NationalId).IsUnique()
            .HasFilter("[VendorAccountKind] = 0 AND [NationalId] IS NOT NULL");
        builder.Entity<User>().Property(u => u.NationalId).HasMaxLength(450);
        builder.Entity<VendorReviewEntry>().ToTable("VendorReviewEntries");
        builder.Entity<VendorReviewEntry>().Property(e => e.Reason).HasMaxLength(1000);
        builder.Entity<VendorReviewEntry>().HasIndex(e => new { e.VendorUserId, e.OccurredAt });
        builder.Entity<VendorReviewEntry>().HasOne<User>().WithMany().HasForeignKey(e => e.VendorUserId).OnDelete(DeleteBehavior.Cascade);

        // استفاده از نام‌های پیش‌فرض Identity (AspNetUsers, AspNetRoles, etc.)
        // اگر می‌خواهید نام‌های سفارشی داشته باشید، خطوط زیر را uncomment کنید:
        // builder.Entity<User>().ToTable("Users");
        // builder.Entity<Role>().ToTable("Roles");
        // builder.Entity<UserRole>().ToTable("UserRoles");
        // builder.Entity<UserClaim>().ToTable("UserClaims");
        // builder.Entity<RoleClaim>().ToTable("RoleClaims");
        // builder.Entity<UserLogin>().ToTable("UserLogins");
        // builder.Entity<UserToken>().ToTable("UserTokens");

        // Configure Navigation Properties to use existing Foreign Keys
        // این کار از ایجاد Foreign Key های اضافی جلوگیری می‌کند
        builder.Entity<RoleClaim>()
            .HasOne(rc => rc.Role)
            .WithMany(r => r.RoleClaims)
            .HasForeignKey(rc => rc.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<UserClaim>()
            .HasOne(uc => uc.User)
            .WithMany()
            .HasForeignKey(uc => uc.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<UserRole>()
            .HasOne(ur => ur.User)
            .WithMany(u => u.UserRoles)
            .HasForeignKey(ur => ur.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<UserRole>()
            .HasOne(ur => ur.Role)
            .WithMany(r => r.UserRoles)
            .HasForeignKey(ur => ur.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<UserLogin>()
            .HasOne(ul => ul.User)
            .WithMany()
            .HasForeignKey(ul => ul.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<UserToken>()
            .HasOne(ut => ut.User)
            .WithMany()
            .HasForeignKey(ut => ut.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<RoleFeature>()
            .HasOne(roleFeature => roleFeature.Feature)
            .WithMany(feature => feature.RoleFeatures)
            .HasForeignKey(roleFeature => roleFeature.FeatureId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<RoleFeature>()
            .HasOne(roleFeature => roleFeature.Role)
            .WithMany(role => role.RoleFeatures)
            .HasForeignKey(roleFeature => roleFeature.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        // Apply configurations
        builder.ApplyConfigurationsFromAssembly(typeof(PMIdentityDbContext).Assembly);

        // Indexes
        builder.Entity<RoleFeature>()
            .HasIndex(roleFeature => new { roleFeature.RoleId, roleFeature.FeatureId })
            .IsUnique();

        builder.Entity<RefreshToken>()
            .HasIndex(rt => rt.Token)
            .IsUnique();

        builder.Entity<UserSession>()
            .HasIndex(us => us.SessionId)
            .IsUnique();

        builder.Entity<PasswordResetToken>()
            .HasIndex(prt => prt.Token)
            .IsUnique();

        builder.Entity<EmailVerificationToken>()
            .HasIndex(evt => evt.Token)
            .IsUnique();
    }
}
