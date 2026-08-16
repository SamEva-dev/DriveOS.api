using FluentValidation;

namespace DriveOS.Modules.Organizations.Application.OrganizationRepresentatives.Suspend;

internal sealed class SuspendOrganizationRepresentativeCommandValidator
    : AbstractValidator<SuspendOrganizationRepresentativeCommand>
{
    public SuspendOrganizationRepresentativeCommandValidator()
    {
        RuleFor(x => x.OrganizationId).NotEmpty();
        RuleFor(x => x.RepresentativeId).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(500);
        RuleFor(x => x.ExpectedRevision).GreaterThan(0);
    }
}
