using FluentValidation;

namespace DriveOS.Modules.Organizations.Application
    .BranchAssignments.CreateBranchUserAssignment;

internal sealed class
    CreateBranchUserAssignmentCommandValidator
    : AbstractValidator<
        CreateBranchUserAssignmentCommand>
{
    public CreateBranchUserAssignmentCommandValidator()
    {
        RuleFor(command =>
                command.OrganizationId.Value)
            .NotEmpty()
            .WithMessage(
                "errors.branchAssignments.organizationId.empty");

        RuleFor(command =>
                command.BranchId.Value)
            .NotEmpty()
            .WithMessage(
                "errors.branchAssignments.branchId.empty");

        RuleFor(command =>
                command.UserId.Value)
            .NotEmpty()
            .WithMessage(
                "errors.branchAssignments.userId.empty");

        RuleFor(command =>
                command.Role)
            .IsInEnum()
            .WithMessage(
                "errors.branchAssignments.role.invalid");

        RuleFor(command =>
                command.AssignmentType)
            .IsInEnum()
            .WithMessage(
                "errors.branchAssignments.type.invalid");

        RuleFor(command =>
                command.PlannedEndAtUtc)
            .Must(value =>
                value is null ||
                value.Value != default)
            .WithMessage(
                "errors.branchAssignments.endDate.invalid");
    }
}