using PM.Identities.Core.Contract.Users.DTOs;
using PM.Identities.Core.Domain.Users;
using Poshteh.Core.ApplicationService.Commands;

namespace PM.Identities.Core.Contract.Users.Commands;

/// <summary>
/// درخواست اصلاح از تأمین‌کننده (دستور مستقیم مالک پروژه 2026/09/07) — مشابه Approve/Reject
/// موجود در VendorsController، اما به‌عنوان اولین Command CQRS این کنترلر. بازگشت خودکار
/// تأمین‌کننده از این وضعیت به «در انتظار بررسی» (پس از اصلاح اطلاعات) به‌صراحت خارج از محدودهٔ
/// این Task است — چون امکان ویرایش خودِ تأمین‌کننده هنوز در سیستم وجود ندارد.
/// </summary>
public sealed record RequestVendorCorrectionCommand(
    long VendorId,
    VendorReviewStatus ExpectedStatus,
    string CorrectionReason,
    long ReviewerId) : ICommand<VendorIdentityDto>;
