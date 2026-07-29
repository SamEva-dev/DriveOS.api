using DriveOS.Modules.Organizations.Domain.Branches;
using FluentValidation;

namespace DriveOS.Modules.Organizations.Application
    .Branches.Lifecycle;

internal sealed class ChangeBranchStatusCommandValidator
    : AbstractValidator<ChangeBranchStatusCommand>
{
    public ChangeBranchStatusCommandValidator()
    {
        RuleFor(command =>
                command.OrganizationId)
            .Must(id => !id.IsEmpty)
            .WithMessage(
                "The organization identifier is required.");

        RuleFor(command =>
                command.BranchId)
            .Must(id => !id.IsEmpty)
            .WithMessage(
                "The branch identifier is required.");

        RuleFor(command =>
                command.Reason)
            .NotEmpty()
            .MaximumLength(
                BranchStatusChangeReason.MaximumLength);

        RuleFor(command =>
                command.TargetStatus)
            .IsInEnum()
            .Must(status =>
                status is
                    BranchStatus.Active or
                    BranchStatus.Restricted or
                    BranchStatus.Suspended or
                    BranchStatus.Closed)
            .WithMessage(
                "The requested target status is not supported.");
    }
}