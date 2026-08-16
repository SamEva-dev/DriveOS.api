using FluentValidation;

namespace DriveOS.Modules.Organizations.Application.Branches.Managers.AssignBranchManager;

internal sealed class AssignBranchManagerCommandValidator
    : AbstractValidator<AssignBranchManagerCommand>
{
    public AssignBranchManagerCommandValidator()
    {
        RuleFor(command => command.OrganizationId.Value)
            .NotEmpty()
            .WithMessage("errors.branches.organizationId.empty");

        RuleFor(command => command.BranchId.Value)
            .NotEmpty()
            .WithMessage("errors.branches.id.empty");

        RuleFor(command => command.ManagerUserId.Value)
            .NotEmpty()
            .WithMessage("errors.branches.manager.userId.empty");

        //RuleFor(command =>
        //        command.EffectiveFromUtc)
        //    .Must(value =>
        //        value is null ||
        //        value.Value != default)
        //    .WithMessage(
        //        "errors.branches.manager.effectiveDate.invalid");
    }
}
