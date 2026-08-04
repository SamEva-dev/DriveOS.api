using DriveOS.Modules.Organizations.Domain.OrganizationSettings;
using FluentValidation;

namespace DriveOS.Modules.Organizations.Application.OrganizationSettings.CreateOrganizationSettings;

public sealed class CreateOrganizationSettingsCommandValidator
    : AbstractValidator<CreateOrganizationSettingsCommand>
{
    public CreateOrganizationSettingsCommandValidator()
    {
        RuleFor(command => command.OrganizationId)
            .Must(id => !id.IsEmpty)
            .WithErrorCode("OrganizationSettings.OrganizationId.Empty")
            .WithMessage("errors.organizationSettings.organizationId.empty");

        RuleFor(command => command.TradeName)
            .MaximumLength(OrganizationProfile.TradeNameMaximumLength);

        RuleFor(command => command.RegistrationNumber)
            .MaximumLength(OrganizationProfile.RegistrationNumberMaximumLength);

        RuleFor(command => command.TaxNumber)
            .MaximumLength(OrganizationProfile.TaxNumberMaximumLength);

        RuleFor(command => command.Email)
            .MaximumLength(OrganizationContactInformation.EmailMaximumLength);

        RuleFor(command => command.Phone)
            .MaximumLength(OrganizationContactInformation.PhoneMaximumLength);

        RuleFor(command => command.Website)
            .MaximumLength(OrganizationContactInformation.WebsiteMaximumLength);

        RuleFor(command => command.AddressCountryCode)
            .NotEmpty()
            .Length(2);

        RuleFor(command => command.DefaultLanguage).NotEmpty();
        RuleFor(command => command.SupportedLanguages).NotEmpty();
        RuleFor(command => command.TimeZoneId).NotEmpty();
        RuleFor(command => command.CurrencyCode).NotEmpty().Length(3);
        RuleFor(command => command.DateFormat).NotEmpty();
        RuleFor(command => command.TimeFormat).NotEmpty();
        RuleFor(command => command.FirstDayOfWeek).IsInEnum();
        RuleFor(command => command.MeasurementSystem).IsInEnum();
    }
}
