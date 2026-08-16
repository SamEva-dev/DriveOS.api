using FluentValidation;

namespace DriveOS.Modules.Organizations.Application.OrganizationSubscriptions.CheckLimit;

public sealed class CheckOrganizationLimitQueryValidator
    : AbstractValidator<CheckOrganizationLimitQuery>
{
    public CheckOrganizationLimitQueryValidator()
    {
        RuleFor(x => x.OrganizationId).Must(x => !x.IsEmpty);
        RuleFor(x => x.LimitCode).NotEmpty();
        RuleFor(x => x.CurrentUsage).GreaterThanOrEqualTo(0);
        RuleFor(x => x.RequestedIncrease).GreaterThanOrEqualTo(0);
    }
}
