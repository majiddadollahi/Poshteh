using PM.Identities.Core.Contract.Users.Commands;
using Poshteh.Core.ApplicationService.Behaviors;
using Poshteh.Core.Domain;
namespace PM.Identities.Core.ApplicationService.Users.Validators;
public sealed class SubmitVendorProfileCommandValidator : IValidator<SubmitVendorProfileCommand>
{
    public Result Validate(SubmitVendorProfileCommand r) => r.UserId > 0 ? Result.Success() : Result.Failure(VendorProfileRules.Forbidden);
}
