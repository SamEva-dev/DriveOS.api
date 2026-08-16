using FluentValidation;

namespace DriveOS.Modules.Organizations.Application.OrganizationRepresentatives.UpdateAuthority;

internal sealed class UpdateOrganizationRepresentativeAuthorityCommandValidator
    : AbstractValidator<UpdateOrganizationRepresentativeAuthorityCommand>
{
    public UpdateOrganizationRepresentativeAuthorityCommandValidator()
    {
        RuleFor(x => x.OrganizationId).NotEmpty();
        RuleFor(x => x.RepresentativeId).NotEmpty();
        RuleFor(x => x.AuthorityScope).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.EffectiveTo)
            .GreaterThanOrEqualTo(x => x.EffectiveFrom)
            .When(x => x.EffectiveTo.HasValue);
        RuleFor(x => x.ExpectedRevision).GreaterThan(0);
    }
}
