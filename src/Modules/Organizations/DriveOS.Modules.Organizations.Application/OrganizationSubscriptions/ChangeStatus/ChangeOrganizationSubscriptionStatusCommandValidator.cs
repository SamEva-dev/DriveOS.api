using FluentValidation;
namespace DriveOS.Modules.Organizations.Application.OrganizationSubscriptions.ChangeStatus;
public sealed class ChangeOrganizationSubscriptionStatusCommandValidator : AbstractValidator<ChangeOrganizationSubscriptionStatusCommand>
{
    public ChangeOrganizationSubscriptionStatusCommandValidator()
    {
        RuleFor(x => x.OrganizationId).Must(x => !x.IsEmpty);
        RuleFor(x => x.TargetStatus).IsInEnum();
        RuleFor(x => x.ExpectedVersion).GreaterThan(0);
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(500);
        RuleFor(x => x.ChangedByUserId).Must(x => !x.IsEmpty);
        When(x => x.TargetStatus == DriveOS.Modules.Organizations.Domain.Subscriptions.SubscriptionStatus.Active, () => RuleFor(x => x.PeriodStartsAtUtc).NotNull());
    }
}
