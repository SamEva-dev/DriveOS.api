using FluentValidation;
namespace DriveOS.Modules.Organizations.Application.BranchConfigurationOverrides.UpdateDraft;
public sealed class UpdateBranchConfigurationOverrideDraftCommandValidator : AbstractValidator<UpdateBranchConfigurationOverrideDraftCommand>
{
    public UpdateBranchConfigurationOverrideDraftCommandValidator()
    {
        RuleFor(x => x.OrganizationId).Must(id => !id.IsEmpty);
        RuleFor(x => x.BranchId).Must(id => !id.IsEmpty);
        RuleFor(x => x.OverrideId).Must(id => !id.IsEmpty);
        RuleFor(x => x.PayloadJson).NotEmpty();
        RuleFor(x => x.ExpectedRevision).GreaterThan(0);
    }
}
