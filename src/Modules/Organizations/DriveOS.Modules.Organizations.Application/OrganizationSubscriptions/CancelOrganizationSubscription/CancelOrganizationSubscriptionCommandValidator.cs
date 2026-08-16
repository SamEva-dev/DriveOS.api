using FluentValidation;

namespace DriveOS.Modules.Organizations.Application.OrganizationSubscriptions.CancelOrganizationSubscription;

public sealed class CancelOrganizationSubscriptionCommandValidator
    : AbstractValidator<CancelOrganizationSubscriptionCommand>
{
    public CancelOrganizationSubscriptionCommandValidator()
    {
        RuleFor(x => x.OrganizationId).Must(x => !x.IsEmpty);
        RuleFor(x => x.ExpectedVersion).GreaterThan(0);
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(500);
        RuleFor(x => x.RequestedByUserId).Must(x => !x.IsEmpty);
        RuleFor(x => x).Must(x => x.EffectiveAtUtc >= x.RequestedAtUtc);
    }
}
