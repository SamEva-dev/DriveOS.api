using FluentValidation;

namespace DriveOS.Modules.Organizations.Application.OrganizationSequences.Suspend;

internal sealed class SuspendOrganizationSequenceCommandValidator
    : AbstractValidator<SuspendOrganizationSequenceCommand>
{
    public SuspendOrganizationSequenceCommandValidator()
    {
        RuleFor(command => command.OrganizationId).NotEmpty();
        RuleFor(command => command.SequenceId).NotEmpty();
        RuleFor(command => command.ExpectedRevision).GreaterThan(0);
    }
}
