using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PM.Identities.Core.Contract.Users.DTOs;
using PM.Procurement.Core.Contract.Integration.BuyerProfiles.DTOs;
using PM.Vendor.Core.Contract.Integration.Companies.DTOs;
using Poshteh.Core.Domain;
using PM.Identities.Core.ApplicationService.TokenService;
using PM.Identities.Core.Contract.Users;
using PM.Identities.Infra.DAL.DbContext;
using PM.Identities.Infra.DAL.Repositories;
using System.Security.Claims;
using PM.Identities.Core.Contract.Auth;
using PM.Identities.Core.Domain.Users;
using PM.Identities.Core.Domain.Errors;
using PM.Procurement.Core.Contract.Integration.BuyerProfiles;
using PM.Vendor.Core.Contract.Integration.Companies;
using PM.Platform.Core.Contract.Notifications;
using PM.Platform.Core.Contract.AuditLogs;

namespace PM.Identities.Core.ApplicationService.Users;

public class UserService : IUserService
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly PMIdentityDbContext _context;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly ITokenService _tokenService;
    private readonly VendorLoginResolver? _vendorLoginResolver;
    private readonly IBuyerProfileReaderContract? _buyerProfileReader;
    private readonly ICompanyReaderContract? _companyReader;
    private readonly IAdminNotificationPublisher? _adminNotifications;
    private readonly IAuditLogWriter? _auditLog;

    public UserService(
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        PMIdentityDbContext context,
        IRefreshTokenRepository refreshTokenRepository,
        ITokenService tokenService, VendorLoginResolver? vendorLoginResolver = null,
        IBuyerProfileReaderContract? buyerProfileReader = null, ICompanyReaderContract? companyReader = null,
        IAdminNotificationPublisher? adminNotifications = null, IAuditLogWriter? auditLog = null)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _context = context;
        _refreshTokenRepository = refreshTokenRepository;
        _tokenService = tokenService;
        _vendorLoginResolver = vendorLoginResolver;
        _buyerProfileReader = buyerProfileReader;
        _companyReader = companyReader;
        _adminNotifications = adminNotifications;
        _auditLog = auditLog;
    }

    public async Task<Result<UserDto>> CreateAsync(CreateUserDto dto, CancellationToken cancellationToken = default)
    {
        if (dto.AccountType is not (AccountType.PanelAdmin or AccountType.PanelUser))
        {
            return Result.Failure<UserDto>(new Error(
                "Identity.InvalidManagedAccountType",
                "از مسیر مدیریت کاربران فقط حساب PanelAdmin یا PanelUser قابل ایجاد است."));
        }

        // Check if user already exists
        var existingUser = await _userManager.FindByNameAsync(dto.UserName);
        if (existingUser != null)
        {
            return Result.Failure<UserDto>(IdentityErrors.UserAlreadyExists(dto.UserName));
        }

        var existingEmail = await _userManager.FindByEmailAsync(dto.Email);
        if (existingEmail != null)
        {
            return Result.Failure<UserDto>(IdentityErrors.EmailAlreadyExists(dto.Email));
        }

        var user = new User
        {
            UserName = dto.UserName,
            Email = dto.Email,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            PhoneNumber = dto.PhoneNumber,
            NationalId = dto.NationalId,
            AccountType = dto.AccountType.Value,
            IsActive = true,
            EmailConfirmed = false
        };

        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
        {
            return Result.Failure<UserDto>(
                IdentityResultMapper.MapWithCodes(result, "Identity.UserCreationFailed"));
        }

        if (_adminNotifications is not null)
        {
            await _adminNotifications.PublishAsync(new AdminNotificationRequest(
                AdminNotificationEventCodes.UserRegistered, NotificationEntityTypes.UserAccount,
                user.Id, null, user.DisplayName), cancellationToken);
        }

        return Result.Success(MapToDto(user));
    }

    public async Task<Result<UserDto>> UpdateAsync(long id, UpdateUserDto dto, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null)
        {
            return Result.Failure<UserDto>(IdentityErrors.UserNotFound(id));
        }

        if (user.AccountType.IsVendor() && (user.NationalId != dto.NationalId ||
            user.FirstName != dto.FirstName || user.LastName != dto.LastName || user.PhoneNumber != dto.PhoneNumber))
            return Result.Failure<UserDto>(new Error("Vendor.ProfileEndpointRequired", "اطلاعات تأمین‌کننده را از صفحه تکمیل پروفایل ویرایش کنید."));
        user.Email = dto.Email;
        user.FirstName = dto.FirstName;
        user.LastName = dto.LastName;
        user.PhoneNumber = dto.PhoneNumber;
        user.NationalId = dto.NationalId;
        user.ProfilePictureUrl = dto.ProfilePictureUrl;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            var errorMessage = string.Join(", ", result.Errors.Select(e => e.Description));
            return Result.Failure<UserDto>(IdentityErrors.UserUpdateFailed(errorMessage));
        }

        if (_adminNotifications is not null && user.AccountType.IsBuyer())
        {
            var buyerProfileId = _buyerProfileReader is null
                ? null
                : await _buyerProfileReader.GetBuyerProfileIdByUserIdAsync(user.Id, cancellationToken);
            await _adminNotifications.PublishAsync(new AdminNotificationRequest(
                AdminNotificationEventCodes.BuyerAccountUpdated, NotificationEntityTypes.BuyerAccount,
                buyerProfileId ?? user.Id, user.Id, user.DisplayName), cancellationToken);
        }

        return Result.Success(MapToDto(user));
    }

    public async Task<Result<UserDto>> UpdateMyProfileAsync(long id, UpdateMyProfileDto dto, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user is null) return Result.Failure<UserDto>(IdentityErrors.UserNotFound(id));

        user.FirstName = dto.FirstName.Trim();
        user.LastName = dto.LastName.Trim();
        user.PhoneNumber = string.IsNullOrWhiteSpace(dto.PhoneNumber)
            ? null
            : IranianIdentifiers.Normalize(dto.PhoneNumber);
        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
            return Result.Failure<UserDto>(IdentityResultMapper.MapWithCodes(result, "Identity.UserUpdateFailed"));

        return Result.Success(MapToDto(user));
    }

    public async Task<Result> DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null)
        {
            return Result.Failure(IdentityErrors.UserNotFound(id));
        }

        var result = await _userManager.DeleteAsync(user);
        if (!result.Succeeded)
        {
            var errorMessage = string.Join(", ", result.Errors.Select(e => e.Description));
            return Result.Failure(IdentityErrors.UserDeleteFailed(errorMessage));
        }

        await _refreshTokenRepository.RevokeAllUserTokensAsync(user.Id, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result<UserDto>> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null)
        {
            return Result.Failure<UserDto>(IdentityErrors.UserNotFound(id));
        }
        return Result.Success(MapToDto(user));
    }

    public async Task<Result<UserDto>> GetByUserNameAsync(string userName, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByNameAsync(userName);
        if (user == null)
        {
            return Result.Failure<UserDto>(IdentityErrors.UserNotFound(userName));
        }
        return Result.Success(MapToDto(user));
    }

    public async Task<Result<UserDto>> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            return Result.Failure<UserDto>(IdentityErrors.UserNotFound(email));
        }
        return Result.Success(MapToDto(user));
    }

    public async Task<Result<PagedUserListDto>> GetListAsync(UserListFilterDto filter, CancellationToken cancellationToken = default)
    {
        var query = _userManager.Users.AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var searchText = filter.Search.Trim();
            query = query.Where(u =>
                (u.UserName != null && u.UserName.Contains(searchText)) ||
                (u.Email != null && u.Email.Contains(searchText)) ||
                u.FirstName.Contains(searchText) ||
                u.LastName.Contains(searchText) ||
                (u.PhoneNumber != null && u.PhoneNumber.Contains(searchText)));
        }

        if (!string.IsNullOrWhiteSpace(filter.UserName))
        {
            query = query.Where(u => u.UserName!.Contains(filter.UserName));
        }

        if (!string.IsNullOrWhiteSpace(filter.Email))
        {
            query = query.Where(u => u.Email!.Contains(filter.Email));
        }

        if (!string.IsNullOrWhiteSpace(filter.FirstName))
        {
            query = query.Where(u => u.FirstName.Contains(filter.FirstName));
        }

        if (!string.IsNullOrWhiteSpace(filter.LastName))
        {
            query = query.Where(u => u.LastName.Contains(filter.LastName));
        }

        if (!string.IsNullOrWhiteSpace(filter.PhoneNumber))
        {
            var phoneNumber = filter.PhoneNumber.Trim();
            query = query.Where(u => u.PhoneNumber != null && u.PhoneNumber.Contains(phoneNumber));
        }

        if (filter.IsActive.HasValue)
        {
            query = query.Where(u => u.IsActive == filter.IsActive.Value);
        }

        var pageNumber = filter.PageNumber < 1 ? 1 : filter.PageNumber;
        var pageSize = filter.PageSize < 1 ? 10 : filter.PageSize;

        var totalCount = await query.CountAsync(cancellationToken);

        var users = await query
            .OrderBy(u => u.UserName)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return Result.Success(new PagedUserListDto
        {
            Items = users.Select(MapToDto).ToList(),
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize,
        });
    }

    /// <summary>
    /// فهرست مدیریت کاربران (TASK-0145): خروجی اختصاصی (<see cref="UserListItemDto"/>) که کاربران
    /// پنل/خریدار/تأمین‌کننده را یکپارچه، با نوع و سازمان واقعی برمی‌گرداند. فیلتر سازمانی طبق الگوی
    /// VendorsController/GetVendorListQuery ابتدا به UserId از ماژول مالک (Procurement/Vendor) حل
    /// می‌شود، سپس همراه بقیهٔ فیلترها روی IQueryable دیتابیس Identity (AsNoTracking) اعمال می‌شود؛
    /// enrichment نمایشی (نام/شناسهٔ سازمان) فقط برای UserIdهای صفحهٔ جاری و به‌شکل bulk انجام می‌شود.
    /// </summary>
    public async Task<Result<PagedUserManagementListDto>> GetManagementListAsync(UserManagementListFilterDto filter, CancellationToken cancellationToken = default)
    {
        var query = _userManager.Users.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var searchText = filter.Search.Trim();
            query = query.Where(u =>
                (u.UserName != null && u.UserName.Contains(searchText)) ||
                (u.Email != null && u.Email.Contains(searchText)) ||
                u.FirstName.Contains(searchText) ||
                u.LastName.Contains(searchText) ||
                (u.PhoneNumber != null && u.PhoneNumber.Contains(searchText)));
        }

        if (!string.IsNullOrWhiteSpace(filter.UserName))
        {
            query = query.Where(u => u.UserName!.Contains(filter.UserName));
        }

        if (!string.IsNullOrWhiteSpace(filter.Email))
        {
            query = query.Where(u => u.Email!.Contains(filter.Email));
        }

        if (!string.IsNullOrWhiteSpace(filter.FirstName))
        {
            query = query.Where(u => u.FirstName.Contains(filter.FirstName));
        }

        if (!string.IsNullOrWhiteSpace(filter.LastName))
        {
            query = query.Where(u => u.LastName.Contains(filter.LastName));
        }

        if (!string.IsNullOrWhiteSpace(filter.PhoneNumber))
        {
            var phoneNumber = filter.PhoneNumber.Trim();
            query = query.Where(u => u.PhoneNumber != null && u.PhoneNumber.Contains(phoneNumber));
        }

        if (filter.IsActive.HasValue)
        {
            query = query.Where(u => u.IsActive == filter.IsActive.Value);
        }

        if (filter.UserType.HasValue)
        {
            query = filter.UserType.Value switch
            {
                UserTypeFamily.Panel => query.Where(u => AccountTypes.Panel.Contains(u.AccountType)),
                UserTypeFamily.Buyer => query.Where(u => AccountTypes.Buyer.Contains(u.AccountType)),
                UserTypeFamily.Vendor => query.Where(u => AccountTypes.Vendor.Contains(u.AccountType)),
                _ => query,
            };
        }

        if (!string.IsNullOrWhiteSpace(filter.OrganizationName))
        {
            var orgNameMatchedIds = await ResolveOrganizationMatchedUserIdsAsync(
                filter.UserType, filter.OrganizationName, legalId: null, cancellationToken);
            query = query.Where(u => orgNameMatchedIds.Contains(u.Id));
        }

        // «کد ملی/شناسه ملی» مؤثر: یا شناسهٔ سازمان واقعی کاربر (Buyer/Vendor) یا، وقتی سازمانی در
        // کار نیست، کد ملی شخصی خود کاربر (User.NationalId) — تا کاربر حقیقی هم با کد ملی پیدا شود.
        if (!string.IsNullOrWhiteSpace(filter.NationalId))
        {
            var nationalId = filter.NationalId.Trim();
            var organizationMatchedIds = await ResolveOrganizationMatchedUserIdsAsync(
                filter.UserType, companyName: null, nationalId, cancellationToken);
            query = query.Where(u =>
                (u.NationalId != null && u.NationalId == nationalId) || organizationMatchedIds.Contains(u.Id));
        }

        var pageNumber = filter.PageNumber < 1 ? 1 : filter.PageNumber;
        var pageSize = filter.PageSize < 1 ? 10 : filter.PageSize;

        var totalCount = await query.CountAsync(cancellationToken);

        var page = await query
            .OrderBy(u => u.UserName)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new
            {
                u.Id,
                u.UserName,
                u.FirstName,
                u.LastName,
                u.Email,
                u.PhoneNumber,
                u.IsActive,
                u.AccountType,
                u.NationalId,
            })
            .ToListAsync(cancellationToken);

        var buyerUserIds = page.Where(u => u.AccountType.IsBuyer()).Select(u => u.Id).ToArray();
        var vendorUserIds = page.Where(u => u.AccountType.IsVendor()).Select(u => u.Id).ToArray();

        var buyerProfiles = buyerUserIds.Length > 0 && _buyerProfileReader is not null
            ? await _buyerProfileReader.GetByUserIdsAsync(buyerUserIds, cancellationToken)
            : new Dictionary<long, BuyerProfileSummaryDto>();

        var companies = vendorUserIds.Length > 0 && _companyReader is not null
            ? await _companyReader.GetBySupplierUserIdsAsync(vendorUserIds, cancellationToken)
            : new Dictionary<long, CompanySummaryDto>();

        var items = page.Select(u =>
        {
            var family = u.AccountType.IsPanel() ? UserTypeFamily.Panel
                : u.AccountType.IsBuyer() ? UserTypeFamily.Buyer
                : UserTypeFamily.Vendor;

            string? organizationName = null;
            string? organizationNationalId = null;
            if (u.AccountType.IsBuyer() && buyerProfiles.TryGetValue(u.Id, out var buyerProfile))
            {
                organizationName = buyerProfile.CompanyName;
                organizationNationalId = buyerProfile.LegalId;
            }
            else if (u.AccountType.IsVendor() && companies.TryGetValue(u.Id, out var company))
            {
                organizationName = company.Name;
                organizationNationalId = company.LegalNationalId;
            }

            var isOrgManager = u.AccountType is AccountType.BuyerOwner or AccountType.BuyerAdmin
                or AccountType.VendorOwner or AccountType.VendorAdmin;
            var personName = $"{u.FirstName} {u.LastName}".Trim();
            var displayName = isOrgManager && !string.IsNullOrWhiteSpace(organizationName)
                ? $"مدیر {organizationName}"
                : !string.IsNullOrWhiteSpace(personName) ? personName : "کاربر";

            var effectiveNationalId = !string.IsNullOrWhiteSpace(organizationNationalId)
                ? organizationNationalId
                : u.NationalId;

            return new UserListItemDto(
                u.Id, u.UserName ?? string.Empty, effectiveNationalId, displayName, family,
                organizationName, u.Email, u.PhoneNumber, u.IsActive);
        }).ToList();

        return Result.Success(new PagedUserManagementListDto
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize,
        });
    }

    /// <summary>
    /// شناسهٔ کاربرهایی که BuyerProfile/Company متعلق‌شان با نام (Contains) یا شناسهٔ ملی/حقوقی
    /// (تطابق دقیق) مطابقت دارد؛ طبق فیلتر نوع کاربر فقط قرارداد مالک مربوط فراخوانی می‌شود.
    /// </summary>
    private async Task<IReadOnlySet<long>> ResolveOrganizationMatchedUserIdsAsync(
        UserTypeFamily? userType, string? companyName, string? legalId, CancellationToken cancellationToken)
    {
        var matchedUserIds = new HashSet<long>();

        if ((userType is null || userType == UserTypeFamily.Buyer) && _buyerProfileReader is not null)
        {
            var buyerMatches = await _buyerProfileReader.FindUserIdsByOrganizationAsync(companyName, legalId, cancellationToken);
            matchedUserIds.UnionWith(buyerMatches);
        }

        if ((userType is null || userType == UserTypeFamily.Vendor) && _companyReader is not null)
        {
            var vendorMatches = await _companyReader.FindSupplierUserIdsByOrganizationAsync(companyName, legalId, cancellationToken);
            matchedUserIds.UnionWith(vendorMatches);
        }

        return matchedUserIds;
    }

    public async Task<Result<List<UserSearchResultDto>>> SearchAsync(string text, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return Result.Success(new List<UserSearchResultDto>());
        }

        var searchText = text.Trim();

        var users = await _userManager.Users
            .Where(u =>
                (u.UserName != null && u.UserName.Contains(searchText)) ||
                (u.Email != null && u.Email.Contains(searchText)) ||
                u.FirstName.Contains(searchText) ||
                u.LastName.Contains(searchText) ||
                (u.PhoneNumber != null && u.PhoneNumber.Contains(searchText)))
            .OrderBy(u => u.UserName)
            .Take(20)
            .ToListAsync(cancellationToken);

        return Result.Success(users.Select(MapToSearchResultDto).ToList());
    }

    public async Task<Result<AuthenticationResultDto>> LoginAsync(LoginDto dto, CancellationToken cancellationToken = default)
    {
        var user = _vendorLoginResolver is null 
            ? await _userManager.FindByNameAsync(dto.EffectiveIdentifier) 
            : await _vendorLoginResolver.ResolveAsync(dto.EffectiveIdentifier, cancellationToken);
        if (user == null)
        {
            return Result.Failure<AuthenticationResultDto>(IdentityErrors.InvalidCredentials);
        }

        if (!user.IsActive)
        {
            return Result.Failure<AuthenticationResultDto>(IdentityErrors.InvalidCredentials);
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, lockoutOnFailure: true);
        if (!result.Succeeded)
        {
            return Result.Failure<AuthenticationResultDto>(IdentityErrors.InvalidCredentials);
        }

        // Update last login date. `user` is tracked by the same scoped _context that UserManager's
        // store uses (both resolve to PMIdentityDbContext in this request), so this change is
        // persisted together with the refresh token below in a single SaveChangesAsync round trip
        // instead of a separate _userManager.UpdateAsync call.
        user.LastLoginDate = DateTime.UtcNow;

        // Generate tokens with role claims for authorization checks.
        var accessToken = _tokenService.GenerateAccessToken(user, await BuildUserClaimsAsync(user));
        var refreshToken = _tokenService.GenerateRefreshToken(user.Id);

        await _refreshTokenRepository.AddAsync(refreshToken, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success(new AuthenticationResultDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken.Token,
            ExpiresAt = DateTime.UtcNow.Add(_tokenService.AccessTokenLifetime),
            User = MapToDto(user)
        });
    }

    public async Task<Result<AuthenticationResultDto>> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var token = await _refreshTokenRepository.GetByTokenAsync(refreshToken, cancellationToken);
        if (token == null)
        {
            return Result.Failure<AuthenticationResultDto>(IdentityErrors.RefreshTokenNotFound);
        }

        if (!token.IsActive)
        {
            return Result.Failure<AuthenticationResultDto>(IdentityErrors.RefreshTokenRevoked);
        }

        var user = await _userManager.FindByIdAsync(token.UserId.ToString());
        if (user == null)
        {
            return Result.Failure<AuthenticationResultDto>(IdentityErrors.UserNotFound(token.UserId));
        }

        if (!user.IsActive)
        {
            return Result.Failure<AuthenticationResultDto>(IdentityErrors.UserInactive);
        }

        // Revoke old token
        token.RevokedAt = DateTime.UtcNow;
        await _refreshTokenRepository.UpdateAsync(token, cancellationToken);

        // Generate new tokens with role claims for authorization checks.
        var newAccessToken = _tokenService.GenerateAccessToken(user, await BuildUserClaimsAsync(user));
        var newRefreshToken = _tokenService.GenerateRefreshToken(user.Id);

        await _refreshTokenRepository.AddAsync(newRefreshToken, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success(new AuthenticationResultDto
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken.Token,
            ExpiresAt = DateTime.UtcNow.Add(_tokenService.AccessTokenLifetime),
            User = MapToDto(user)
        });
    }

    public async Task<Result> LogoutAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var token = await _refreshTokenRepository.GetByTokenAsync(refreshToken, cancellationToken);
        if (token != null)
        {
            token.RevokedAt = DateTime.UtcNow;
            await _refreshTokenRepository.UpdateAsync(token, cancellationToken);
           await _context.SaveChangesAsync(cancellationToken);
        }
        return Result.Success();
    }

    public async Task<Result> ChangePasswordAsync(long userId, ChangePasswordDto dto, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            return Result.Failure(IdentityErrors.UserNotFound(userId));
        }

        var result = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
        if (!result.Succeeded)
        {
            if (result.Errors.Any(e => e.Code == "PasswordMismatch"))
            {
                return Result.Failure(IdentityErrors.PasswordMismatch);
            }
            return Result.Failure(
                IdentityResultMapper.MapWithCodes(result, "Identity.PasswordChangeFailed"));
        }

        await _refreshTokenRepository.RevokeAllUserTokensAsync(user.Id, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> AdminResetPasswordAsync(
        long userId,
        AdminResetPasswordDto dto,
        long? actorUserId = null,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            return Result.Failure(IdentityErrors.UserNotFound(userId));
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, token, dto.Password);
        if (!result.Succeeded)
        {
            return Result.Failure(
                IdentityResultMapper.MapWithCodes(result, "Identity.PasswordResetFailed"));
        }

        await _refreshTokenRepository.RevokeAllUserTokensAsync(user.Id, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        if (_auditLog is not null)
        {
            await _auditLog.WriteAsync("Identity.User.AdminPasswordReset", actorUserId,
                "User", user.Id.ToString(), cancellationToken: cancellationToken);
        }

        return Result.Success();
    }

    public async Task<Result> RequestPasswordResetAsync(string email, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            // Don't reveal if user exists - return success anyway
            return Result.Success();
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        // TODO: Send email with token
        return Result.Success();
    }

    public async Task<Result> ResetPasswordAsync(ResetPasswordDto dto, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null)
        {
            return Result.Failure(IdentityErrors.InvalidToken);
        }

        var result = await _userManager.ResetPasswordAsync(user, dto.Token, dto.NewPassword);
        if (!result.Succeeded)
        {
            return Result.Failure(
                IdentityResultMapper.MapWithCodes(result, "Identity.PasswordResetFailed"));
        }

        await _refreshTokenRepository.RevokeAllUserTokensAsync(user.Id, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> RequestEmailVerificationAsync(long userId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            return Result.Failure(IdentityErrors.UserNotFound(userId));
        }

        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        // TODO: Send email with token
        return Result.Success();
    }

    public async Task<Result> VerifyEmailAsync(string token, CancellationToken cancellationToken = default)
    {
        // TODO: Implement email verification
        return Result.Failure(IdentityErrors.NotImplemented);
    }

    public async Task<Result> ActivateUserAsync(long userId, long? actorUserId = null, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            return Result.Failure(IdentityErrors.UserNotFound(userId));
        }

        user.IsActive = true;
        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            var errorMessage = string.Join(", ", result.Errors.Select(e => e.Description));
            return Result.Failure(IdentityErrors.UserActivationFailed(errorMessage));
        }

        if (_auditLog is not null)
        {
            await _auditLog.WriteAsync("Identity.User.Activated", actorUserId,
                "User", user.Id.ToString(), cancellationToken: cancellationToken);
        }

        return Result.Success();
    }

    public async Task<Result> DeactivateUserAsync(long userId, long? actorUserId = null, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            return Result.Failure(IdentityErrors.UserNotFound(userId));
        }

        user.IsActive = false;
        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            var errorMessage = string.Join(", ", result.Errors.Select(e => e.Description));
            return Result.Failure(IdentityErrors.UserDeactivationFailed(errorMessage));
        }

        if (_auditLog is not null)
        {
            await _auditLog.WriteAsync("Identity.User.Deactivated", actorUserId,
                "User", user.Id.ToString(), cancellationToken: cancellationToken);
        }

        return Result.Success();
    }

    private static UserSearchResultDto MapToSearchResultDto(User user) => new()
    {
        Id = user.Id,
        UserName = user.UserName ?? string.Empty,
        FirstName = user.FirstName,
        LastName = user.LastName,
        FullName = user.DisplayName,
        DisplayName = user.DisplayName,
        AccountType = user.AccountType
    };

    private static UserDto MapToDto(User user)
    {
        return new UserDto
        {
            Id = user.Id,
            UserName = user.UserName ?? string.Empty,
            Email = user.Email ?? string.Empty,
            PhoneNumber = user.PhoneNumber,
            FirstName = user.FirstName,
            LastName = user.LastName,
            FullName = user.DisplayName,
            DisplayName = user.DisplayName,
            AccountType = user.AccountType,
            NationalId = user.NationalId,
            IsActive = user.IsActive,
            EmailConfirmed = user.EmailConfirmed,
            LastLoginDate = user.LastLoginDate,
            ProfilePictureUrl = user.ProfilePictureUrl,
            CreatedDate = DateTime.UtcNow, // باید از BaseEntity خوانده شود
            ReviewStatus = user.VendorReviewStatus,
            RejectionReason = user.VendorRejectionReason,
            CorrectionReason = user.VendorCorrectionReason
        };
    }

    /// <summary>
    /// طبق دستور مالک پروژه: BuyerId/SupplierId + یک عکس‌فوری IsActive داخل Claims توکن قرار
    /// می‌گیرد — صرفاً برای UX سریع Frontend (از جمله رفع مشکل «از دست رفتن isActive بعد از رفرش
    /// صفحه»، چون بازیابی نشست از روی توکن ذخیره‌شده هیچ HTTP Call ای انجام نمی‌دهد). Backend این
    /// Claim را هرگز برای تصمیم امنیتی معتبر نمی‌داند — هر بررسی واقعی (Policyهای ActiveBuyer/
    /// ActiveSupplier) همیشه مستقیم از دیتابیس می‌خواند، چون ممکن است بعد از صدور توکن مدیر سامانه
    /// وضعیت را تغییر داده باشد. همان الگوی دسترسی بین‌ماژولی VendorLoginResolver (که همین‌جا،
    /// همین کلاس، از قبل به IBuyerProfileReaderContract/PM.Vendor.Core.Contract وابسته است).
    /// </summary>
    private async Task<List<Claim>> BuildUserClaimsAsync(User user)
    {
        var claims = new List<Claim>();
        var roles = await _userManager.GetRolesAsync(user);
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        if (_buyerProfileReader is not null && user.AccountType.IsBuyer())
        {
            var buyerProfile = await _buyerProfileReader.GetByUserIdAsync(user.Id);
            if (buyerProfile is not null)
            {
                claims.Add(new Claim(MahtaJwtClaimTypes.BuyerId, buyerProfile.Id.ToString()));
                claims.Add(new Claim(MahtaJwtClaimTypes.IsActive, buyerProfile.IsActive.ToString()));
            }
        }

        // TASK-0020: وضعیت فعال/غیرفعال تأمین‌کننده اکنون از همان lifecycle واحد (VendorReviewStatus،
        // Identities) می‌آید، نه Company.IsActive (حذف‌شده) — برای تأمین‌کنندهٔ حقیقی و حقوقی یکسان.
        if (user.AccountType.IsVendor())
        {
            claims.Add(new Claim(MahtaJwtClaimTypes.IsActive, (user.VendorReviewStatus == VendorReviewStatus.Active).ToString()));
            if (_companyReader is not null)
            {
                var company = await _companyReader.GetBySupplierUserIdAsync(user.Id);
                if (company is not null)
                {
                    claims.Add(new Claim(MahtaJwtClaimTypes.SupplierId, company.Id.ToString()));
                }
            }
        }

        return claims;
    }
}
