using BuildingBlocks.Data.Migrations;
using PM.Identities.Core.ApplicationService.Users.Handlers;
using PM.Identities.Core.ApplicationService.Users.Validators;
using PM.Identities.Core.Contract.Users.Commands;
using PM.Identities.EndpointApi.Authorization;
using PM.Identities.Core.ApplicationService.Accesses;
using PM.Identities.Core.ApplicationService.Features;
using PM.Identities.Core.ApplicationService.Integration.Accounts;
using PM.Identities.Core.ApplicationService.Integration.Users;
using PM.Identities.Core.ApplicationService.Roles;
using PM.Identities.Core.ApplicationService.TokenService;
using PM.Identities.Core.ApplicationService.Users;
using PM.Identities.Core.ApplicationService.Integration.Notifications;
using PM.Identities.Core.ApplicationService.Localization;
using PM.Platform.Core.Contract.Notifications;
using PM.Identities.Core.Contract.Accesses;
using PM.Identities.Core.Contract.Features;
using PM.Identities.Core.Contract.Integration.Accounts;
using PM.Identities.Core.Contract.Integration.Users;
using PM.Identities.Core.Contract.Roles;
using PM.Identities.Core.Contract.Users;
using PM.Identities.Infra.DAL.DbContext;
using PM.Identities.Infra.DAL.Repositories;
using PM.Identities.Infra.DAL.Bootstrap;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using PM.Identities.Core.Domain.Users;
using PM.Identities.Core.Domain.Roles;
using Poshteh.Core.ApplicationService.Behaviors;
using Poshteh.Core.ApplicationService.DependencyInjection;
using Poshteh.Shared.Kernel.Time;

namespace PM.Identities.EndpointApi.Extensions;

public static class IdentityServiceCollectionExtensions
{
    /// <summary>
    /// ثبت تمام وابستگی‌های ماژول Identity
    /// </summary>
    public static IServiceCollection AddIdentityModule(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        // Server-time service (BR-15/NFR-TIME-01): shared across all modules, registered once.
        services.TryAddSingleton<IClock, SystemClock>();

        // Register DbContext
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        services.AddDbContext<PMIdentityDbContext>(options =>
            options.UseSqlServer(connectionString,
                sql => sql.MigrationsHistoryTable(SharedMigrationsHistory.TableName, SharedMigrationsHistory.SchemaName)));
        services.Configure<BootstrapAdminOptions>(
            configuration.GetSection(BootstrapAdminOptions.SectionName));
        services.AddScoped<BootstrapAdminInitializer>();

        // Register ASP.NET Identity Core (without Authentication Scheme)
        // استفاده از AddIdentityCore به جای AddIdentity برای جلوگیری از تداخل با JWT Authentication
        var identityBuilder = services.AddIdentityCore<User>(options =>
        {
            // Password settings
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequiredLength = 6;

            options.Lockout.AllowedForNewUsers = true;
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            
            // User settings
            options.User.RequireUniqueEmail = true;
        })
        .AddRoles<Role>()
        .AddEntityFrameworkStores<PMIdentityDbContext>()
        .AddDefaultTokenProviders();
        
        // Register SignInManager - Authentication قبلاً در Host ثبت شده است
        services.AddHttpContextAccessor();
        identityBuilder.AddSignInManager();
        services.RemoveAll<IUserValidator<User>>();
        services.AddScoped<IUserValidator<User>, VendorIdentityUserValidator>();
        services.AddScoped<VendorLoginResolver>();
        services.AddScoped<VendorWorkflow>();
        services.AddScoped<IValidator<UpdateVendorProfileCommand>, UpdateVendorProfileCommandValidator>();
        services.AddScoped<IValidator<SubmitVendorProfileCommand>, SubmitVendorProfileCommandValidator>();
        services.AddScoped<IValidator<ReviewVendorCommand>, ReviewVendorCommandValidator>();

        // Register UnitOfWork
        //services.AddScoped<IUnitOfWork>(sp =>
        //{
        //    var dbContext = sp.GetRequiredService<MahtaIdentityDbContext>();
        //    return new UnitofWork(dbContext);
        //});

        // Register Repositories
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

        // Register Application Services
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<IAccessService, AccessService>();
        services.AddScoped<IAccessCatalogSyncService, AccessCatalogSyncService>();
        services.AddScoped<IFeatureService, FeatureService>();
        services.AddScoped<IRoleFeatureService, RoleFeatureService>();
        services.AddSingleton<IAdminAccessCache, AdminAccessCache>();
        services.AddSingleton<IAccessRegistryCache, AccessRegistryCache>();
        services.AddScoped<IAdminAccessCacheInvalidator, AdminAccessCacheInvalidator>();
        services.AddScoped<IAdminAccessEvaluator, AdminAccessEvaluator>();
        // TASK-0247: scoped on purpose — member state is memoized for one request only.
        services.AddScoped<IPMAuthorizationService, PMAuthorizationService>();
        services.AddScoped<BuildingBlocks.Web.Authorization.IBuyerAccountChecker, BuyerAccountChecker>();
        services.AddScoped<IAdminAccessQueryService, AdminAccessQueryService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IIdentityActiveAccountCheckerContract, IdentityActiveAccountCheckerService>();
        services.AddScoped<IIdentityAccountCreatorContract, IdentityAccountCreatorService>();
        services.AddScoped<IIdentityAccountReaderContract, IdentityAccountReaderService>();
        services.AddScoped<IIdentityUserProfileReaderContract, IdentityUserProfileReaderService>();
        services.AddScoped<IIdentityUserProfilesReaderContract, IdentityUserProfilesReaderService>();
        services.AddScoped<IIdentityAdminWorkReaderContract, PM.Identities.Core.ApplicationService.Integration.Users.IdentityAdminWorkReaderService>();

        // BL-010: اولین استفادهٔ این ماژول از الگوی CQRS پوشته (سرویس‌های قدیمی بالا دست‌نخورده ماندند).
        // PMIdentityDbContext یک BaseCommandDbContext نیست (ASP.NET Identity ported از srcold)،
        // پس IUnitOfWork/TransactionBehavior برای این ماژول کاربردی ندارد؛ RegisterBuyerCommand
        // به‌جای آن مستقیماً از UserManager (که خودش SaveChanges را انجام می‌دهد) استفاده می‌کند.
        services.AddApplicationService(
            typeof(IdentityServiceCollectionExtensions).Assembly,
            typeof(RegisterBuyerCommandHandler).Assembly);
        services.AddPoshtehMessageResources<IdentityMessages>("Identity.");
        // IValidator<T> به‌صورت خودکار Scan نمی‌شود (محدودیت شناخته‌شدهٔ Poshteh) — باید دستی ثبت شود.
        services.AddScoped<IValidator<RegisterBuyerCommand>, RegisterBuyerCommandValidator>();
        services.AddScoped<IAdminNotificationRecipientResolver, AdminNotificationRecipientResolver>();
        // BL-038: ثبت‌نام تأمین‌کننده (حقیقی/حقوقی، BR-34) — همان الگوی RegisterBuyerCommand.
        services.AddScoped<IValidator<RegisterVendorCommand>, RegisterVendorCommandValidator>();
        // درخواست اصلاح از تأمین‌کننده (دستور مستقیم مالک پروژه 2026/09/07).
        services.AddScoped<IValidator<RequestVendorCorrectionCommand>, RequestVendorCorrectionCommandValidator>();

        // Note: Controllers will be registered in Host via AddModuleControllers

        return services;
    }
}

