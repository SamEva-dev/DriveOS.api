using DriveOS.Modules.Organizations.Domain.Organizations;
using FluentValidation;

namespace DriveOS.Modules.Organizations.Application.Organizations.Lifecycle;

internal sealed class ChangeOrganizationStatusCommandValidator
    : AbstractValidator<ChangeOrganizationStatusCommand>
{
    public ChangeOrganizationStatusCommandValidator()
    {
        RuleFor(command => command.OrganizationId).NotEmpty();

        RuleFor(command => command.Reason)
            .NotEmpty()
            .MaximumLength(OrganizationStatusChangeReason.MaximumLength);

        RuleFor(command => command.TargetStatus)
            .IsInEnum()
            .Must(status =>
                status
                    is OrganizationStatus.PendingActivation
                        or OrganizationStatus.Active
                        or OrganizationStatus.Restricted
                        or OrganizationStatus.Suspended
                        or OrganizationStatus.Closed
            )
            .WithMessage("The requested target status is not supported.");
    }
}
