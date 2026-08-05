using FluentValidation;

namespace DriveOS.Modules.Organizations.Application.OrganizationRepresentatives.Create;

internal sealed class CreateOrganizationRepresentativeCommandValidator
    : AbstractValidator<CreateOrganizationRepresentativeCommand>
{
    public CreateOrganizationRepresentativeCommandValidator()
    {
        RuleFor(x => x.OrganizationId).NotEmpty();
        RuleFor(x => x.PersonId).NotEmpty();
        RuleFor(x => x.RepresentativeType).IsInEnum();
        RuleFor(x => x.AuthorityScope).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.EffectiveTo)
            .GreaterThanOrEqualTo(x => x.EffectiveFrom)
            .When(x => x.EffectiveTo.HasValue);
        RuleFor(x => x.IsPrimaryOwner)
            .Equal(false)
            .When(x => x.RepresentativeType != Domain.OrganizationRepresentatives.OrganizationRepresentativeType.Owner);
    }
}
