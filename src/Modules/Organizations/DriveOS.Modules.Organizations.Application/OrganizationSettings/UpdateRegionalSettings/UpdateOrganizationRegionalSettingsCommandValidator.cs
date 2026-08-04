using FluentValidation;

namespace DriveOS.Modules.Organizations.Application.OrganizationSettings.UpdateRegionalSettings;

public sealed class UpdateOrganizationRegionalSettingsCommandValidator
    : AbstractValidator<UpdateOrganizationRegionalSettingsCommand>
{
    public UpdateOrganizationRegionalSettingsCommandValidator()
    {
        RuleFor(x => x.OrganizationId).Must(id => !id.IsEmpty);
        RuleFor(x => x.DefaultLanguage).NotEmpty();
        RuleFor(x => x.SupportedLanguages).NotEmpty();
        RuleFor(x => x.TimeZoneId).NotEmpty();
        RuleFor(x => x.CurrencyCode).NotEmpty().Length(3);
        RuleFor(x => x.DateFormat).NotEmpty();
        RuleFor(x => x.TimeFormat).NotEmpty();
        RuleFor(x => x.FirstDayOfWeek).IsInEnum();
        RuleFor(x => x.MeasurementSystem).IsInEnum();
        RuleFor(x => x.ExpectedVersion).GreaterThan(0);
    }
}
