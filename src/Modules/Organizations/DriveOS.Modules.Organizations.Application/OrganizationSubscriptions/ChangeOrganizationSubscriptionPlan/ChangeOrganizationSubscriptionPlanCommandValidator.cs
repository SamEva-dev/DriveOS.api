using FluentValidation;
namespace DriveOS.Modules.Organizations.Application.OrganizationSubscriptions.ChangeOrganizationSubscriptionPlan;
public sealed class ChangeOrganizationSubscriptionPlanCommandValidator : AbstractValidator<ChangeOrganizationSubscriptionPlanCommand>
{
 public ChangeOrganizationSubscriptionPlanCommandValidator(){RuleFor(x=>x.OrganizationId).Must(x=>!x.IsEmpty);RuleFor(x=>x.PlanCode).NotEmpty().MaximumLength(80);RuleFor(x=>x.ExpectedVersion).GreaterThan(0);RuleFor(x=>x.Reason).NotEmpty().MaximumLength(500);RuleFor(x=>x.ChangedByUserId).Must(x=>!x.IsEmpty);RuleFor(x=>x.EntitlementCodes).Must(x=>x.Distinct(StringComparer.Ordinal).Count()==x.Count);RuleFor(x=>x.Limits).Must(x=>x.All(i=>i.Value>=0));}
}
