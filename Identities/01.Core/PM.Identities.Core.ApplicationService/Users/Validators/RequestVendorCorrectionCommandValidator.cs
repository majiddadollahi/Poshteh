using PM.Identities.Core.Contract.Users.Commands;
using PM.Identities.Core.Domain.Errors;
using Poshteh.Core.ApplicationService.Behaviors;
using Poshteh.Core.Domain;

namespace PM.Identities.Core.ApplicationService.Users.Validators;

/// <summary>طبق قانون ثابت CQRS منشور همکاری: هر Command باید CommandValidator اختصاصی داشته باشد.</summary>
public sealed class RequestVendorCorrectionCommandValidator : IValidator<RequestVendorCorrectionCommand>
{
    public Result Validate(RequestVendorCorrectionCommand request)
    {
        if (request.VendorId <= 0)
        {
            return Result.Failure(IdentityErrors.IdentifierRequired);
        }

        if (string.IsNullOrWhiteSpace(request.CorrectionReason))
        {
            return Result.Failure(IdentityErrors.VendorCorrectionReasonRequired);
        }

        if (request.CorrectionReason.Trim().Length > 1000)
        {
            return Result.Failure(IdentityErrors.VendorCorrectionReasonTooLong);
        }

        return Result.Success();
    }
}
