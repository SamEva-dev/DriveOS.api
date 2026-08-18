using FluentValidation;
namespace DriveOS.Modules.FundingBilling.Application.BillingAccounts.Create;
public sealed class CreateBillingAccountCommandValidator : AbstractValidator<CreateBillingAccountCommand>
{
    public CreateBillingAccountCommandValidator()
    {
        RuleFor(x => x.OrganizationId.Value).NotEmpty();
        RuleFor(x => x.StudentId.Value).NotEmpty();
        RuleFor(x => x.ActorUserId.Value).NotEmpty();
        RuleFor(x => x.Currency).NotEmpty().Length(3).Matches("^[A-Za-z]{3}$");
    }
}
