using FluentValidation;
namespace DriveOS.Modules.Organizations.Application.OrganizationLegalProfiles.Activate;
internal sealed class ActivateOrganizationLegalProfileCommandValidator : AbstractValidator<ActivateOrganizationLegalProfileCommand>
{
    public ActivateOrganizationLegalProfileCommandValidator(){ RuleFor(x=>x.OrganizationId).NotEmpty(); RuleFor(x=>x.ExpectedRevision).GreaterThan(0); }
}
