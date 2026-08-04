using FluentValidation;

namespace DriveOS.Modules.Organizations.Application.OrganizationConfigurations.Archive;

public sealed class ArchiveOrganizationConfigurationCommandValidator
    : AbstractValidator<ArchiveOrganizationConfigurationCommand>
{
    public ArchiveOrganizationConfigurationCommandValidator()
    {
        RuleFor(command => command.OrganizationId).Must(id => !id.IsEmpty);
        RuleFor(command => command.ConfigurationId).Must(id => !id.IsEmpty);
        RuleFor(command => command.ExpectedRevision).GreaterThan(0);
    }
}
