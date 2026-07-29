using FluentValidation;

namespace DriveOS.Modules.Organizations.Application.Branches.SetPrimaryBranch;

internal sealed class SetPrimaryBranchCommandValidator :
    AbstractValidator<SetPrimaryBranchCommand>
{
    public SetPrimaryBranchCommandValidator()
    {
        RuleFor(command => command.OrganizationId)
            .Must(id => !id.IsEmpty);

        RuleFor(command => command.BranchId)
            .Must(id => !id.IsEmpty);
    }
}
