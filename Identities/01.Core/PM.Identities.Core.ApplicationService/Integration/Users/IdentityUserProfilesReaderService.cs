using PM.Identities.Core.Contract.Integration.Users.DTOs;
using Poshteh.Core.Domain;
using PM.Identities.Core.Contract.Integration.Users;
using PM.Identities.Infra.DAL.DbContext;
using Microsoft.EntityFrameworkCore;
using PM.Identities.Core.Domain.Users;

namespace PM.Identities.Core.ApplicationService.Integration.Users;

public class IdentityUserProfilesReaderService : IIdentityUserProfilesReaderContract
{
    private readonly PMIdentityDbContext _context;

    public IdentityUserProfilesReaderService(PMIdentityDbContext context)
    {
        _context = context;
    }

    public async Task<Result<IReadOnlyList<IdentityUserProfileDto>>> GetUserProfilesAsync(
        IReadOnlyCollection<long> userIds,
        CancellationToken cancellationToken = default)
    {
        if (userIds == null || userIds.Count == 0)
        {
            return Result.Success<IReadOnlyList<IdentityUserProfileDto>>(Array.Empty<IdentityUserProfileDto>());
        }

        var distinctUserIds = userIds.Where(x => x > 0).Distinct().ToList();
        if (distinctUserIds.Count == 0)
        {
            return Result.Success<IReadOnlyList<IdentityUserProfileDto>>(Array.Empty<IdentityUserProfileDto>());
        }

        var users = await _context.Users
            .AsNoTracking()
            .Where(x => distinctUserIds.Contains(x.Id))
            .ToListAsync(cancellationToken);

        var profiles = users
            .Select(IdentityUserProfileMapper.Map)
            .ToList();

        return Result.Success<IReadOnlyList<IdentityUserProfileDto>>(profiles);
    }

    public async Task<Result<IReadOnlyList<IdentityUserProfileDto>>> GetVendorProfilesByReviewStatusAsync(
        IReadOnlyCollection<VendorReviewStatus> reviewStatuses,
        CancellationToken cancellationToken = default)
    {
        if (reviewStatuses.Count == 0)
        {
            return Result.Success<IReadOnlyList<IdentityUserProfileDto>>(Array.Empty<IdentityUserProfileDto>());
        }

        var users = await _context.Users
            .AsNoTracking()
            .Where(user => AccountTypes.Vendor.Contains(user.AccountType) && reviewStatuses.Contains(user.VendorReviewStatus))
            .ToListAsync(cancellationToken);

        var profiles = users
            .Select(IdentityUserProfileMapper.Map)
            .ToList();

        return Result.Success<IReadOnlyList<IdentityUserProfileDto>>(profiles);
    }

    public async Task<Result<IReadOnlyList<IdentityUserProfileDto>>> SearchActiveVendorProfilesAsync(
        string? name,
        string? nationalId,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Users.AsNoTracking()
            .Where(user => user.IsActive
                && user.VendorReviewStatus == VendorReviewStatus.Active
                && AccountTypes.Vendor.Contains(user.AccountType));

        if (!string.IsNullOrWhiteSpace(name))
        {
            var term = name.Trim();
            query = query.Where(user =>
                ((user.FirstName ?? string.Empty) + " " + (user.LastName ?? string.Empty)).Contains(term)
                || (user.UserName != null && user.UserName.Contains(term)));
        }

        if (!string.IsNullOrWhiteSpace(nationalId))
        {
            var id = nationalId.Trim();
            query = query.Where(user => user.NationalId == id);
        }

        var rows = await query
            .Select(user => new IdentityUserProfileDto
            {
                UserId = user.Id,
                UserName = user.UserName ?? string.Empty,
                FirstName = user.FirstName,
                LastName = user.LastName,
                NationalId = user.NationalId,
                IsActive = user.IsActive,
                ReviewStatus = user.VendorReviewStatus,
            })
            .ToListAsync(cancellationToken);

        foreach (var row in rows)
        {
            row.FullName = $"{row.FirstName} {row.LastName}".Trim();
        }

        return Result.Success<IReadOnlyList<IdentityUserProfileDto>>(rows);
    }

    public async Task<Result<EligibleSupplierProfilePageDto>> SearchEligibleSuppliersAsync(
        string? searchText,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Users.AsNoTracking()
            .Where(user => user.IsActive
                && user.VendorReviewStatus == VendorReviewStatus.Active
                && AccountTypes.Vendor.Contains(user.AccountType));

        if (!string.IsNullOrWhiteSpace(searchText))
        {
            var term = searchText.Trim();
            query = query.Where(user => (user.FirstName + " " + user.LastName).Contains(term)
                || (user.UserName != null && user.UserName.Contains(term)));
        }

        var normalizedPageNumber = Math.Max(pageNumber, 1);
        var normalizedPageSize = Math.Clamp(pageSize, 1, 50);
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query.OrderBy(user => user.FirstName).ThenBy(user => user.LastName).ThenBy(user => user.Id)
            .Skip((normalizedPageNumber - 1) * normalizedPageSize)
            .Take(normalizedPageSize)
            .Select(user => new EligibleSupplierProfileDto(
                user.Id,
                ((user.FirstName ?? string.Empty) + " " + (user.LastName ?? string.Empty)).Trim()))
            .ToListAsync(cancellationToken);

        return Result.Success(new EligibleSupplierProfilePageDto(
            items, totalCount, normalizedPageNumber, normalizedPageSize));
    }
}
