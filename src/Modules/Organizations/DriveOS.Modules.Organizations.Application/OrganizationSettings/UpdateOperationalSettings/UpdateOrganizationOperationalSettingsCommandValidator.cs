using DriveOS.Modules.Organizations.Domain.OrganizationSettings;
using FluentValidation;

namespace DriveOS.Modules.Organizations.Application.OrganizationSettings.UpdateOperationalSettings;

public sealed class UpdateOrganizationOperationalSettingsCommandValidator
    : AbstractValidator<UpdateOrganizationOperationalSettingsCommand>
{
    public UpdateOrganizationOperationalSettingsCommandValidator()
    {
        RuleFor(x => x.OrganizationId).Must(id => !id.IsEmpty);
        RuleFor(x => x.ExpectedVersion).GreaterThan(0);
        RuleFor(x => x.DefaultSessionDurationMinutes)
            .InclusiveBetween(OrganizationOperationalSettings.MinimumSessionDurationMinutes,
                OrganizationOperationalSettings.MaximumSessionDurationMinutes);
        RuleFor(x => x.DefaultBookingLeadTimeMinutes)
            .InclusiveBetween(0, OrganizationOperationalSettings.MaximumBookingLeadTimeMinutes);
        RuleFor(x => x.DefaultCancellationDelayHours)
            .InclusiveBetween(0, OrganizationOperationalSettings.MaximumCancellationDelayHours);
    }
}
