using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PM.Identities.Core.Contract.Users.Commands;
using PM.Identities.Core.Contract.Users.DTOs;
using PM.Identities.Core.Contract.Users.Queries;
using PM.Identities.Core.Domain.Users;
using PM.Identities.EndpointApi.Base;
using PM.Vendor.Core.Contract.Integration.Companies;
using PM.Vendor.Core.Contract.Integration.Companies.DTOs;
using Poshteh.Core.ApplicationService.Commands;
using Poshteh.Core.ApplicationService.Queries;

namespace PM.Identities.EndpointApi.Endpoints;

[Route("identity/[controller]")]
public sealed class VendorsController(UserManager<User> users, ICompanyReaderContract companies,
    IQueryDispatcher queryDispatcher, ICommandDispatcher commandDispatcher) : BaseApiController
{
    private IQueryable<User> Vendors => users.Users.Where(u => AccountTypes.Vendor.Contains(u.AccountType));

    [HttpGet]
    public async Task<IActionResult> GetList([FromQuery] string? name, [FromQuery] string? nationalId,
        [FromQuery] string? phoneNumber, [FromQuery] VendorTypeFilter? vendorType,
        [FromQuery] VendorReviewStatus? status, [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        if (status.HasValue && !Enum.IsDefined(status.Value)) return BadRequest();
        if (vendorType.HasValue && !Enum.IsDefined(vendorType.Value)) return BadRequest();

        IReadOnlySet<long>? legalVendorUserIds = null;
        if (vendorType.HasValue)
        {
            legalVendorUserIds = await companies.GetAllSupplierUserIdsAsync(cancellationToken);
        }

        var listResult = await queryDispatcher.Send(new GetVendorListQuery(
            name, nationalId, phoneNumber, status,
            vendorType.HasValue ? vendorType.Value == VendorTypeFilter.Legal : null,
            legalVendorUserIds, pageNumber, pageSize), cancellationToken);
        if (listResult.IsFailure) return HandleResult(listResult);

        var page = listResult.Value;
        var companyProfiles = await companies.GetBySupplierUserIdsAsync(page.Items.Select(u => u.Id).ToArray(), cancellationToken);
        return OkResponse(new VendorPageDto(
            page.Items.Select(u => ToDto(u, companyProfiles.GetValueOrDefault(u.Id))).ToArray(),
            page.TotalCount, page.PageNumber, page.PageSize));
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetById(long id, CancellationToken cancellationToken)
    {
        var user = await Vendors.AsNoTracking().SingleOrDefaultAsync(u => u.Id == id, cancellationToken);
        if (user is null) return NotFound();
        return OkResponse(ToDto(user, await companies.GetBySupplierUserIdAsync(id, cancellationToken)));
    }

    [HttpPost("{id:long}/approve")]
    public Task<IActionResult> Approve(long id, VendorReviewRequest request, CancellationToken cancellationToken) =>
        Review(id, request, VendorReviewStatus.Active, cancellationToken);

    [HttpPost("{id:long}/reject")]
    public Task<IActionResult> Reject(long id, VendorReviewRequest request, CancellationToken cancellationToken) =>
        Review(id, request, VendorReviewStatus.Rejected, cancellationToken);

    [HttpPost("{id:long}/request-correction")]
    public async Task<IActionResult> RequestCorrection(long id, VendorCorrectionRequest request, CancellationToken cancellationToken)
    {
        return await Review(id, new VendorReviewRequest(request.ExpectedStatus, request.CorrectionReason), VendorReviewStatus.PendingCorrection, cancellationToken);
    }

    /// <summary>غیرفعال‌سازی کسب‌وکاری تأمین‌کننده (حقیقی/حقوقی) توسط مدیر سامانه — جانشین CompanyProfileController.Deactivate قدیمی (TASK-0020)، بر اساس Vendor UserId نه Company Id.</summary>
    [HttpPost("{id:long}/deactivate")]
    public async Task<IActionResult> Deactivate(long id, CancellationToken cancellationToken)
    {
        if (!long.TryParse(User.FindFirstValue("UserId") ?? User.FindFirstValue(ClaimTypes.NameIdentifier), out var adminUserId)) return Unauthorized();
        var result = await commandDispatcher.Send(new DeactivateVendorCommand(id, adminUserId), cancellationToken);
        if (result.IsFailure) return HandleResult(result);
        return await GetById(id, cancellationToken);
    }

    /// <summary>فعال‌سازی مجدد تأمین‌کنندهٔ غیرفعال‌شده توسط مدیر سامانه (TASK-0020).</summary>
    [HttpPost("{id:long}/activate")]
    public async Task<IActionResult> Activate(long id, CancellationToken cancellationToken)
    {
        if (!long.TryParse(User.FindFirstValue("UserId") ?? User.FindFirstValue(ClaimTypes.NameIdentifier), out var adminUserId)) return Unauthorized();
        var result = await commandDispatcher.Send(new ActivateVendorCommand(id, adminUserId), cancellationToken);
        if (result.IsFailure) return HandleResult(result);
        return await GetById(id, cancellationToken);
    }

    private async Task<IActionResult> Review(long id, VendorReviewRequest request, VendorReviewStatus status, CancellationToken cancellationToken)
    {
        if (!long.TryParse(User.FindFirstValue("UserId") ?? User.FindFirstValue(ClaimTypes.NameIdentifier), out var reviewerId)) return Unauthorized();
        var result = await commandDispatcher.Send(new ReviewVendorCommand(id, reviewerId, request.ExpectedStatus, status, request.RejectionReason), cancellationToken);
        if (result.IsFailure) return HandleResult(result);
        return await GetById(id, cancellationToken);
    }

    private static VendorProfileDto ToDto(User user, CompanySummaryDto? company) => new(
        user.Id, company is null ? "individual" : "legal", company?.Name ?? user.DisplayName,
        user.DisplayName, user.NationalId, user.PhoneNumber, company?.Address,
        company?.RegistrationNumber, company?.EconomicCode, company?.PhoneNumber,
        user.VendorReviewStatus, user.VendorRejectionReason, user.VendorCorrectionReason, user.VendorReviewedAt,
        user.UserName ?? string.Empty, user.Email, user.IsActive, user.RegisteredAt, user.LastLoginDate);

    private static VendorProfileDto ToDto(VendorIdentityDto user, CompanySummaryDto? company) => new(
        user.Id, company is null ? "individual" : "legal", company?.Name ?? user.FullName,
        user.FullName, user.NationalId, user.PhoneNumber, company?.Address,
        company?.RegistrationNumber, company?.EconomicCode, company?.PhoneNumber,
        user.Status, user.RejectionReason, user.CorrectionReason, user.ReviewedAt,
        user.UserName, user.Email, user.IsActive, user.RegisteredAt, user.LastLoginDate);
}
