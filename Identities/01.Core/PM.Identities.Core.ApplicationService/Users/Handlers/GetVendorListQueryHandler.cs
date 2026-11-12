using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PM.Identities.Core.Contract.Users.DTOs;
using PM.Identities.Core.Contract.Users.Queries;
using PM.Identities.Core.Domain.Users;
using Poshteh.Core.ApplicationService.Queries;
using Poshteh.Core.Domain;

namespace PM.Identities.Core.ApplicationService.Users.Handlers;

public sealed class GetVendorListQueryHandler(UserManager<User> userManager)
    : IQueryHandler<GetVendorListQuery, VendorIdentityPageDto>
{
    public async Task<Result<VendorIdentityPageDto>> Handle(GetVendorListQuery query, CancellationToken cancellationToken = default)
    {
        var vendors = userManager.Users.AsNoTracking()
            .Where(u => AccountTypes.Vendor.Contains(u.AccountType));

        if (!string.IsNullOrWhiteSpace(query.Name))
        {
            var name = query.Name.Trim();
            vendors = vendors.Where(u => (u.FirstName + " " + u.LastName).Contains(name));
        }

        if (!string.IsNullOrWhiteSpace(query.NationalId))
        {
            var nationalId = query.NationalId.Trim();
            vendors = vendors.Where(u => u.NationalId != null && u.NationalId.Contains(nationalId));
        }

        if (!string.IsNullOrWhiteSpace(query.PhoneNumber))
        {
            var phoneNumber = query.PhoneNumber.Trim();
            vendors = vendors.Where(u => u.PhoneNumber != null && u.PhoneNumber.Contains(phoneNumber));
        }

        if (query.Status.HasValue)
        {
            vendors = vendors.Where(u => u.VendorReviewStatus == query.Status.Value);
        }

        if (query.IsLegalVendor.HasValue)
        {
            var legalVendorUserIds = query.LegalVendorUserIds ?? new HashSet<long>();
            vendors = query.IsLegalVendor.Value
                ? vendors.Where(u => legalVendorUserIds.Contains(u.Id))
                : vendors.Where(u => !legalVendorUserIds.Contains(u.Id));
        }

        var pageNumber = query.PageNumber < 1 ? 1 : query.PageNumber;
        var pageSize = Math.Clamp(query.PageSize, 1, 100);

        var totalCount = await vendors.CountAsync(cancellationToken);
        var page = await vendors
            .OrderBy(u => u.VendorReviewStatus).ThenByDescending(u => u.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new VendorIdentityDto(u.Id, u.FirstName, u.LastName, u.NationalId, u.PhoneNumber,
                u.VendorReviewStatus, u.VendorRejectionReason, u.VendorCorrectionReason, u.VendorReviewedAt,
                u.IsActive, u.UserName ?? string.Empty, u.Email, u.RegisteredAt, u.LastLoginDate))
            .ToListAsync(cancellationToken);

        return Result.Success(new VendorIdentityPageDto(page, totalCount, pageNumber, pageSize));
    }
}
