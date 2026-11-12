using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using PM.Identities.Core.Contract.Users;
using PM.Identities.Core.Contract.Users.Commands;
using PM.Identities.EndpointApi.Base;
using PM.Identities.EndpointApi.Models;
using Poshteh.Core.ApplicationService.Commands;
using Poshteh.Core.Domain;
using PM.Procurement.Core.Contract.BuyerProfiles.Commands;

namespace PM.Identities.EndpointApi.Endpoints;

[Route("identity/[controller]")]
[ApiController]
[Tags("Identity")]
public class AuthController : BaseApiController
{
    private readonly ICommandDispatcher _commandDispatcher;
    private readonly IUserService _userService;

    public AuthController(ICommandDispatcher commandDispatcher, IUserService userService)
        : base(commandDispatcher)
    {
        _commandDispatcher = commandDispatcher;
        _userService = userService;
    }

    /// <summary>
    /// ثبت‌نام مستقل خریدار (FR-AUTH-01, FR-AUTH-04). ابتدا حساب کاربری (User) در ماژول Identities
    /// ساخته می‌شود، سپس با موفقیت آن، اطلاعات حقوقی شرکت خریدار (BuyerProfile, ENT-18) در ماژول
    /// Procurement ساخته و به همان کاربر پیوند می‌خورد. اگر گام دوم شکست بخورد، کاربر ساخته‌شده
    /// در گام اول جبرانی حذف می‌شود تا حساب بدون پروفایل باقی نماند.
    /// بدون OTP/تأیید مدیر (FR-AUTH-03 به Task آینده موکول شد).
    /// </summary>
    [HttpPost("register-buyer")]
    [EnableRateLimiting("authentication")]
    public async Task<IActionResult> RegisterBuyer(
        [FromBody] RegisterBuyerRequestDto dto,
        CancellationToken cancellationToken)
    {
        var registerResult = await _commandDispatcher.Send(
            new RegisterBuyerCommand(dto.UserName, dto.Email, dto.PhoneNumber, dto.Password, dto.LegalId, dto.ConfirmPassword),
            cancellationToken);

        if (registerResult.IsFailure)
        {
            return HandleResult(registerResult);
        }

        var userId = registerResult.Value;

        var profileResult = await _commandDispatcher.Send(
            new CreateBuyerProfileCommand(
                userId,
                dto.CompanyName,
                dto.LegalId,
                dto.Address,
                dto.CompanyPhoneNumber),
            cancellationToken);

        if (profileResult.IsFailure)
        {
            await _userService.DeleteAsync(userId, cancellationToken);
            return HandleResult(Result.Failure<RegisterBuyerResponseDto>(profileResult.Error));
        }

        return HandleResult(Result.Success(new RegisterBuyerResponseDto(userId, profileResult.Value)));
    }

    /// <summary>
    /// ثبت‌نام مستقل تأمین‌کننده (FR-AUTH-01, FR-AUTH-05, BR-34). ابتدا حساب کاربری (User) در
    /// ماژول Identities با نوع VendorOwner ساخته می‌شود. اگر <c>dto.IsLegalEntity</c>
    /// باشد، سپس اطلاعات حقوقی شرکت (Company, ENT-03) در ماژول Vendor هم ساخته و به همان کاربر
    /// پیوند می‌خورد؛ اگر گام دوم شکست بخورد، کاربر ساخته‌شده در گام اول جبرانی حذف می‌شود تا
    /// حساب بدون شرکت باقی نماند. برای تأمین‌کنندهٔ حقیقی، گام دوم اصلاً اجرا نمی‌شود.
    /// بدون OTP/تأیید مدیر (FR-AUTH-03 به Task آینده موکول شد).
    /// </summary>
    [HttpPost("register-vendor")]
    [EnableRateLimiting("authentication")]
    public Task<IActionResult> RegisterVendor(
        [FromBody] RegisterVendorRequestDto dto,
        CancellationToken cancellationToken)
    {
        return CreateCommand(new RegisterVendorCommand(dto.UserName, dto.Email,
            dto.PhoneNumber, dto.Password, dto.FirstName, dto.LastName, dto.IsLegalEntity,
            dto.NationalCode, dto.LegalNationalId, dto.CompanyName, dto.Address, dto.ConfirmPassword,
            dto.CompanyPhoneNumber), cancellationToken);
    }
}
