using FluentValidation;

namespace DriveOS.Modules.Organizations.Application.OrganizationSequences.Reserve;

internal sealed class ReserveOrganizationSequenceNumberCommandValidator
    : AbstractValidator<ReserveOrganizationSequenceNumberCommand>
{
    public ReserveOrganizationSequenceNumberCommandValidator()
    {
        RuleFor(command => command.OrganizationId).NotEmpty();
        RuleFor(command => command.Code).NotEmpty().MaximumLength(50);
    }
}
