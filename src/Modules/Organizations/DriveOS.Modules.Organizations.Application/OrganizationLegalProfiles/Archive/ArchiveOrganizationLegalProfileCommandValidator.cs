using FluentValidation;

namespace DriveOS.Modules.Organizations.Application.OrganizationLegalProfiles.Archive;

internal sealed class ArchiveOrganizationLegalProfileCommandValidator
    : AbstractValidator<ArchiveOrganizationLegalProfileCommand>
{
    public ArchiveOrganizationLegalProfileCommandValidator()
    {
        RuleFor(x => x.OrganizationId).NotEmpty();
        RuleFor(x => x.ExpectedRevision).GreaterThan(0);
    }
}
