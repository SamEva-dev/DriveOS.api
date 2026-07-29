using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Organizations.Domain.Organizations;

public static class OrganizationErrors
{
    public static readonly Error EmptyId =
        Error.Validation(
            code: "Organizations.Id.Empty",
            messageKey: "errors.organizations.id.empty");

    public static readonly Error EmptyLegalName =
        Error.Validation(
            code: "Organizations.LegalName.Empty",
            messageKey: "errors.organizations.legalName.empty");

    public static Error LegalNameTooLong(int maxLength) =>
        Error.Validation(
            code: "Organizations.LegalName.TooLong",
            messageKey: "errors.organizations.legalName.tooLong",
            parameters: new Dictionary<string, object?>
            {
                ["maxLength"] = maxLength
            });

    public static readonly Error InvalidCountryCode =
        Error.Validation(
            code: "Organizations.CountryCode.Invalid",
            messageKey: "errors.organizations.countryCode.invalid");

    public static readonly Error InvalidOrganizationType =
        Error.Validation(
            code: "Organizations.Type.Invalid",
            messageKey: "errors.organizations.type.invalid");

    public static readonly Error LegalNameAlreadyExists =
        Error.Conflict(
            code: "Organizations.LegalName.AlreadyExists",
            messageKey: "errors.organizations.legalName.alreadyExists");

    public static readonly Error NotFound =
        Error.NotFound(
            code: "Organizations.NotFound",
            messageKey: "errors.organizations.notFound");

    public static readonly Error InvalidId =
    Error.Validation(
        code: "Organizations.Id.Invalid",
        messageKey: "errors.organizations.id.invalid");

    public static readonly Error CurrentUserRequired =
        Error.Unauthorized(
            code: "Organizations.CurrentUser.Required",
            messageKey: "errors.authentication.required");

    public static Error NotFoundById(OrganizationId id) =>
        Error.NotFound(
            "Organizations.NotFound",
            $"The organization '{id}' was not found.");

    public static Error InvalidStatusTransition(
        OrganizationStatus currentStatus,
        OrganizationStatus requestedStatus) =>
        Error.Conflict(
            "Organizations.InvalidStatusTransition",
            $"The organization cannot transition from " +
            $"'{currentStatus}' to '{requestedStatus}'.");
}