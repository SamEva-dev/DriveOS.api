using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Organizations.Domain.OrganizationConfigurations;

public static class OrganizationConfigurationErrors
{
    public static readonly Error EmptyId = Error.Validation(
        "OrganizationConfigurations.Id.Empty",
        "errors.organizationConfiguration.id.empty"
    );

    public static readonly Error EmptyOrganizationId = Error.Validation(
        "OrganizationConfigurations.OrganizationId.Empty",
        "errors.organizationConfiguration.organizationId.empty"
    );

    public static readonly Error InvalidVersion = Error.Validation(
        "OrganizationConfigurations.Version.Invalid",
        "errors.organizationConfiguration.version.invalid"
    );

    public static readonly Error InvalidCountryCode = Error.Validation(
        "OrganizationConfigurations.CountryCode.Invalid",
        "errors.organizationConfiguration.countryCode.invalid"
    );

    public static readonly Error EmptyPayload = Error.Validation(
        "OrganizationConfigurations.Payload.Empty",
        "errors.organizationConfiguration.payload.empty"
    );

    public static readonly Error InvalidPayload = Error.Validation(
        "OrganizationConfigurations.Payload.InvalidJson",
        "errors.organizationConfiguration.payload.invalidJson"
    );

    public static readonly Error PayloadMustBeObject = Error.Validation(
        "OrganizationConfigurations.Payload.MustBeObject",
        "errors.organizationConfiguration.payload.mustBeObject"
    );

    public static Error PayloadTooLong(int maximumLength) =>
        Error.Validation(
            "OrganizationConfigurations.Payload.TooLong",
            "errors.organizationConfiguration.payload.tooLong",
            new Dictionary<string, object?> { ["maximumLength"] = maximumLength }
        );

    public static readonly Error InvalidEffectivePeriod = Error.Validation(
        "OrganizationConfigurations.EffectivePeriod.Invalid",
        "errors.organizationConfiguration.effectivePeriod.invalid"
    );

    public static readonly Error DraftRequired = Error.Conflict(
        "OrganizationConfigurations.Draft.Required",
        "errors.organizationConfiguration.draft.required"
    );

    public static readonly Error PublishedRequired = Error.Conflict(
        "OrganizationConfigurations.Published.Required",
        "errors.organizationConfiguration.published.required"
    );

    public static readonly Error AlreadyPublished = Error.Conflict(
        "OrganizationConfigurations.AlreadyPublished",
        "errors.organizationConfiguration.alreadyPublished"
    );

    public static readonly Error VersionAlreadyExists = Error.Conflict(
        "OrganizationConfigurations.Version.AlreadyExists",
        "errors.organizationConfiguration.version.alreadyExists"
    );

    public static readonly Error NotFound = Error.NotFound(
        "OrganizationConfigurations.NotFound",
        "errors.organizationConfiguration.notFound"
    );

    public static readonly Error CurrentUserRequired = Error.Unauthorized(
        "OrganizationConfigurations.CurrentUser.Required",
        "errors.organizationConfiguration.currentUser.required"
    );

    public static readonly Error ConcurrentUpdate = Error.Conflict(
        "OrganizationConfigurations.ConcurrentUpdate",
        "errors.organizationConfiguration.concurrentUpdate"
    );
}
