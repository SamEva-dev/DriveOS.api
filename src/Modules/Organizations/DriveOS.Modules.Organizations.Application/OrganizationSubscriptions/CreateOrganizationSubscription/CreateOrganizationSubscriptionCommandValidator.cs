using FluentValidation;

namespace DriveOS.Modules.Organizations.Application.OrganizationSubscriptions.CreateOrganizationSubscription;

public sealed class CreateOrganizationSubscriptionCommandValidator
    : AbstractValidator<CreateOrganizationSubscriptionCommand>
{
    public CreateOrganizationSubscriptionCommandValidator()
    {
        RuleFor(x => x.OrganizationId).Must(x => !x.IsEmpty);
        RuleFor(x => x.PlanCode).NotEmpty().MaximumLength(80);
        RuleFor(x => x.Status).IsInEnum();
        RuleFor(x => x.BillingCycle).IsInEnum();
        RuleFor(x => x)
            .Must(x =>
                !x.CurrentPeriodEndsAtUtc.HasValue
                || x.CurrentPeriodEndsAtUtc > x.CurrentPeriodStartsAtUtc
            );
        RuleFor(x => x)
            .Must(x =>
                x.Status != Domain.Subscriptions.SubscriptionStatus.Trialing
                || (x.TrialStartsAtUtc.HasValue && x.TrialEndsAtUtc.HasValue)
            );
        RuleFor(x => x.ExternalProvider).MaximumLength(80);
        RuleFor(x => x.ExternalSubscriptionId).MaximumLength(160);
    }
}
