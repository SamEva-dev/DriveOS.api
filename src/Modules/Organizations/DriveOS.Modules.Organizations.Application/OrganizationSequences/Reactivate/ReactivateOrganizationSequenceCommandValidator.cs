using FluentValidation;

namespace DriveOS.Modules.Organizations.Application.OrganizationSequences.Reactivate;

internal sealed class ReactivateOrganizationSequenceCommandValidator
    : AbstractValidator<ReactivateOrganizationSequenceCommand>
{
    public ReactivateOrganizationSequenceCommandValidator()
    {
        RuleFor(command => command.OrganizationId).NotEmpty();
        RuleFor(command => command.SequenceId).NotEmpty();
        RuleFor(command => command.ExpectedRevision).GreaterThan(0);
    }
}
