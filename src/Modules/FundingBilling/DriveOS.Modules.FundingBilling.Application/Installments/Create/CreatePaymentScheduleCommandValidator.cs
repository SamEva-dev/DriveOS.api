using FluentValidation;

namespace DriveOS.Modules.FundingBilling.Application.Installments.Create;

internal sealed class CreatePaymentScheduleCommandValidator : AbstractValidator<CreatePaymentScheduleCommand>
{
    public CreatePaymentScheduleCommandValidator()
    {
        RuleFor(x => x.BillingAccountId.Value).NotEmpty();
        RuleFor(x => x.ActorUserId.Value).NotEmpty();
        RuleFor(x => x.Installments).NotEmpty().Must(x => x.Count <= 120);
        RuleForEach(x => x.Installments).ChildRules(item =>
        {
            item.RuleFor(x => x.DueDate).NotEqual(default(DateOnly));
            item.RuleFor(x => x.ExpectedAmount).GreaterThan(0m);
            item.RuleFor(x => x).Must(x => !(x.FinancingPersonId.HasValue && x.FinancingOrganizationId.HasValue))
                .WithMessage("Only one financing party can be specified per installment.");
        });
    }
}
