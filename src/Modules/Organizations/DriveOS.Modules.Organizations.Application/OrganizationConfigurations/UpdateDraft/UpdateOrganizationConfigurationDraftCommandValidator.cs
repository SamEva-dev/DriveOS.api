using FluentValidation;

namespace DriveOS.Modules.Organizations.Application.OrganizationConfigurations.UpdateDraft;

public sealed class UpdateOrganizationConfigurationDraftCommandValidator
    : AbstractValidator<UpdateOrganizationConfigurationDraftCommand>
{
    public UpdateOrganizationConfigurationDraftCommandValidator()
    {
        RuleFor(command => command.OrganizationId).Must(id => !id.IsEmpty);
        RuleFor(command => command.ConfigurationId).Must(id => !id.IsEmpty);
        RuleFor(command => command.PayloadJson).NotEmpty();
        RuleFor(command => command.ExpectedRevision).GreaterThan(0);
    }
}
