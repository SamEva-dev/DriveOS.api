using FluentValidation;

namespace DriveOS.Modules.FundingBilling.Application.BillingParties.Manage;

internal sealed class AddBillingPartyCommandValidator : AbstractValidator<AddBillingPartyCommand>
{
    public AddBillingPartyCommandValidator()
    {
        RuleFor(x => x.BillingAccountId.Value).NotEmpty(); RuleFor(x => x.ActorUserId.Value).NotEmpty(); RuleFor(x => x.EffectiveFrom).NotEqual(default(DateOnly)); RuleFor(x => x.Priority).InclusiveBetween(1, 100);
        RuleFor(x => x).Must(x => x.PersonId.HasValue ^ x.PartyOrganizationId.HasValue).WithMessage("Exactly one financial party must be specified.");
        RuleFor(x => x).Must(x => !x.EffectiveTo.HasValue || x.EffectiveTo.Value >= x.EffectiveFrom).WithMessage("EffectiveTo cannot precede EffectiveFrom.");
        RuleFor(x => x.MaximumAmount).GreaterThan(0m).When(x => x.MaximumAmount.HasValue);
    }
}
internal sealed class EndBillingPartyCommandValidator : AbstractValidator<EndBillingPartyCommand>
{
    public EndBillingPartyCommandValidator(){ RuleFor(x=>x.BillingPartyId.Value).NotEmpty(); RuleFor(x=>x.Reason).NotEmpty().MinimumLength(3).MaximumLength(1000); RuleFor(x=>x.ActorUserId.Value).NotEmpty(); }
}
