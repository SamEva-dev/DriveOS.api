using FluentValidation;
namespace DriveOS.Modules.Organizations.Application.BranchConfigurationOverrides.Archive;
public sealed class ArchiveBranchConfigurationOverrideCommandValidator : AbstractValidator<ArchiveBranchConfigurationOverrideCommand>
{
    public ArchiveBranchConfigurationOverrideCommandValidator()
    {
        RuleFor(x => x.OrganizationId).Must(id => !id.IsEmpty);
        RuleFor(x => x.BranchId).Must(id => !id.IsEmpty);
        RuleFor(x => x.OverrideId).Must(id => !id.IsEmpty);
        RuleFor(x => x.ExpectedRevision).GreaterThan(0);
    }
}
