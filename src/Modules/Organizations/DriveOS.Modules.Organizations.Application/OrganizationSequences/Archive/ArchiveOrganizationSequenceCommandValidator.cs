using FluentValidation;

namespace DriveOS.Modules.Organizations.Application.OrganizationSequences.Archive;

internal sealed class ArchiveOrganizationSequenceCommandValidator
    : AbstractValidator<ArchiveOrganizationSequenceCommand>
{
    public ArchiveOrganizationSequenceCommandValidator()
    {
        RuleFor(command => command.OrganizationId).NotEmpty();
        RuleFor(command => command.SequenceId).NotEmpty();
        RuleFor(command => command.ExpectedRevision).GreaterThan(0);
    }
}
