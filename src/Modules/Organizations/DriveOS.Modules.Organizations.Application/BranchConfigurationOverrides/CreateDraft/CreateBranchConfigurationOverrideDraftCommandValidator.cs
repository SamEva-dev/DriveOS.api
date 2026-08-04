using FluentValidation;

namespace DriveOS.Modules.Organizations.Application.BranchConfigurationOverrides.CreateDraft;

public sealed class CreateBranchConfigurationOverrideDraftCommandValidator
    : AbstractValidator<CreateBranchConfigurationOverrideDraftCommand>
{
    public CreateBranchConfigurationOverrideDraftCommandValidator()
    {
        RuleFor(command => command.OrganizationId)
            .Must(id => !id.IsEmpty)
            .WithErrorCode("BranchConfigurationOverrides.OrganizationId.Empty")
            .WithMessage("errors.branchConfigurationOverride.organizationId.empty");

        RuleFor(command => command.BranchId)
            .Must(id => !id.IsEmpty)
            .WithErrorCode("BranchConfigurationOverrides.BranchId.Empty")
            .WithMessage("errors.branchConfigurationOverride.branchId.empty");

        RuleFor(command => command.BaseConfigurationId)
            .Must(id => !id.IsEmpty)
            .WithErrorCode("BranchConfigurationOverrides.BaseConfigurationId.Empty")
            .WithMessage("errors.branchConfigurationOverride.baseConfigurationId.empty");

        RuleFor(command => command.VersionNumber)
            .GreaterThan(0)
            .WithErrorCode("BranchConfigurationOverrides.Version.Invalid")
            .WithMessage("errors.branchConfigurationOverride.version.invalid");

        RuleFor(command => command.CountryCode)
            .NotEmpty().Length(2).Matches("^[A-Za-z]{2}$")
            .WithErrorCode("BranchConfigurationOverrides.CountryCode.Invalid")
            .WithMessage("errors.branchConfigurationOverride.countryCode.invalid");

        RuleFor(command => command.PayloadJson)
            .NotEmpty()
            .WithErrorCode("BranchConfigurationOverrides.Payload.Empty")
            .WithMessage("errors.branchConfigurationOverride.payload.empty");
    }
}
