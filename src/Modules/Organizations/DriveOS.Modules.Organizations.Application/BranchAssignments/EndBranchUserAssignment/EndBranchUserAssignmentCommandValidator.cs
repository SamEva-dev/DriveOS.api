using DriveOS.Modules.Organizations.Domain
    .BranchAssignments;
using FluentValidation;

namespace DriveOS.Modules.Organizations.Application
    .BranchAssignments.EndBranchUserAssignment;

internal sealed class
    EndBranchUserAssignmentCommandValidator
    : AbstractValidator<
        EndBranchUserAssignmentCommand>
{
    public EndBranchUserAssignmentCommandValidator()
    {
        RuleFor(command =>
                command.OrganizationId.Value)
            .NotEmpty()
            .WithMessage(
                "errors.branchAssignments.organizationId.empty");

        RuleFor(command =>
                command.AssignmentId.Value)
            .NotEmpty()
            .WithMessage(
                "errors.branchAssignments.id.empty");

        RuleFor(command =>
                command.Reason)
            .NotEmpty()
            .MaximumLength(
                BranchAssignmentReason
                    .MaximumLength)
            .WithMessage(
                "errors.branchAssignments.reason.invalid");
    }
}