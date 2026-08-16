using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Organizations.Domain.OrganizationSettings;

public static class OrganizationSettingsErrors
{
    public static readonly Error EmptyId = Error.Validation(
        "OrganizationSettings.Id.Empty",
        "errors.organizationSettings.id.empty"
    );

    public static readonly Error EmptyOrganizationId = Error.Validation(
        "OrganizationSettings.OrganizationId.Empty",
        "errors.organizationSettings.organizationId.empty"
    );

    public static readonly Error NotFound = Error.NotFound(
        "OrganizationSettings.NotFound",
        "errors.organizationSettings.notFound"
    );

    public static readonly Error AlreadyExists = Error.Conflict(
        "OrganizationSettings.AlreadyExists",
        "errors.organizationSettings.alreadyExists"
    );

    public static readonly Error InvalidTradeName = Error.Validation(
        "OrganizationSettings.Profile.TradeName.Invalid",
        "errors.organizationSettings.profile.tradeName.invalid"
    );

    public static readonly Error InvalidRegistrationNumber = Error.Validation(
        "OrganizationSettings.Profile.RegistrationNumber.Invalid",
        "errors.organizationSettings.profile.registrationNumber.invalid"
    );

    public static readonly Error InvalidTaxNumber = Error.Validation(
        "OrganizationSettings.Profile.TaxNumber.Invalid",
        "errors.organizationSettings.profile.taxNumber.invalid"
    );

    public static readonly Error InvalidEmail = Error.Validation(
        "OrganizationSettings.Contact.Email.Invalid",
        "errors.organizationSettings.contact.email.invalid"
    );

    public static readonly Error InvalidPhone = Error.Validation(
        "OrganizationSettings.Contact.Phone.Invalid",
        "errors.organizationSettings.contact.phone.invalid"
    );

    public static readonly Error InvalidWebsite = Error.Validation(
        "OrganizationSettings.Contact.Website.Invalid",
        "errors.organizationSettings.contact.website.invalid"
    );

    public static readonly Error InvalidAddress = Error.Validation(
        "OrganizationSettings.Address.Invalid",
        "errors.organizationSettings.address.invalid"
    );

    public static readonly Error IncompleteAddress = Error.Validation(
        "OrganizationSettings.Address.Incomplete",
        "errors.organizationSettings.address.incomplete"
    );

    public static readonly Error InvalidAddressCountryCode = Error.Validation(
        "OrganizationSettings.Address.CountryCode.Invalid",
        "errors.organizationSettings.address.countryCode.invalid"
    );

    public static readonly Error InvalidLanguages = Error.Validation(
        "OrganizationSettings.Regional.Languages.Invalid",
        "errors.organizationSettings.regional.languages.invalid"
    );

    public static readonly Error DefaultLanguageNotSupported = Error.Validation(
        "OrganizationSettings.Regional.DefaultLanguage.NotSupported",
        "errors.organizationSettings.regional.defaultLanguage.notSupported"
    );

    public static readonly Error InvalidTimeZone = Error.Validation(
        "OrganizationSettings.Regional.TimeZone.Invalid",
        "errors.organizationSettings.regional.timeZone.invalid"
    );

    public static readonly Error InvalidCurrency = Error.Validation(
        "OrganizationSettings.Regional.Currency.Invalid",
        "errors.organizationSettings.regional.currency.invalid"
    );

    public static readonly Error InvalidDateTimeFormat = Error.Validation(
        "OrganizationSettings.Regional.DateTimeFormat.Invalid",
        "errors.organizationSettings.regional.dateTimeFormat.invalid"
    );

    public static readonly Error InvalidRegionalConvention = Error.Validation(
        "OrganizationSettings.Regional.Convention.Invalid",
        "errors.organizationSettings.regional.convention.invalid"
    );

    public static readonly Error InvalidSessionDuration = Error.Validation(
        "OrganizationSettings.Operational.SessionDuration.Invalid",
        "errors.organizationSettings.operational.sessionDuration.invalid"
    );

    public static readonly Error InvalidBookingLeadTime = Error.Validation(
        "OrganizationSettings.Operational.BookingLeadTime.Invalid",
        "errors.organizationSettings.operational.bookingLeadTime.invalid"
    );

    public static readonly Error InvalidCancellationDelay = Error.Validation(
        "OrganizationSettings.Operational.CancellationDelay.Invalid",
        "errors.organizationSettings.operational.cancellationDelay.invalid"
    );

    public static readonly Error InvalidDefaultBranch = Error.Validation(
        "OrganizationSettings.Operational.DefaultBranch.Invalid",
        "errors.organizationSettings.operational.defaultBranch.invalid"
    );

    public static readonly Error DefaultBranchRequired = Error.Validation(
        "OrganizationSettings.Operational.DefaultBranch.Required",
        "errors.organizationSettings.operational.defaultBranch.required"
    );

    public static readonly Error DefaultBranchNotOwned = Error.Conflict(
        "OrganizationSettings.Operational.DefaultBranch.NotOwned",
        "errors.organizationSettings.operational.defaultBranch.notOwned"
    );

    public static readonly Error ConcurrentUpdate = Error.Conflict(
        "OrganizationSettings.ConcurrentUpdate",
        "errors.organizationSettings.concurrentUpdate"
    );
}
