using FluentValidation;

namespace DriveOS.Modules.Organizations.Application.OrganizationRepresentatives.Activate;

internal sealed class ActivateOrganizationRepresentativeCommandValidator
    : AbstractValidator<ActivateOrganizationRepresentativeCommand>
{
    public ActivateOrganizationRepresentativeCommandValidator()
    {
        RuleFor(x => x.OrganizationId).NotEmpty();
        RuleFor(x => x.RepresentativeId).NotEmpty();
        RuleFor(x => x.ExpectedRevision).GreaterThan(0);
    }
}
