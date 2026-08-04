using FluentValidation;

namespace DriveOS.Modules.Organizations.Application.OrganizationConfigurations.CreateDraft;

public sealed class CreateOrganizationConfigurationDraftCommandValidator
    : AbstractValidator<CreateOrganizationConfigurationDraftCommand>
{
    public CreateOrganizationConfigurationDraftCommandValidator()
    {
        RuleFor(command => command.OrganizationId)
            .Must(id => !id.IsEmpty)
            .WithErrorCode("OrganizationConfigurations.OrganizationId.Empty")
            .WithMessage("errors.organizationConfiguration.organizationId.empty");

        RuleFor(command => command.VersionNumber)
            .GreaterThan(0)
            .WithErrorCode("OrganizationConfigurations.Version.Invalid")
            .WithMessage("errors.organizationConfiguration.version.invalid");

        RuleFor(command => command.CountryCode)
            .NotEmpty()
            .Length(2)
            .Matches("^[A-Za-z]{2}$")
            .WithErrorCode("OrganizationConfigurations.CountryCode.Invalid")
            .WithMessage("errors.organizationConfiguration.countryCode.invalid");

        RuleFor(command => command.PayloadJson)
            .NotEmpty()
            .WithErrorCode("OrganizationConfigurations.Payload.Empty")
            .WithMessage("errors.organizationConfiguration.payload.empty");
    }
}
