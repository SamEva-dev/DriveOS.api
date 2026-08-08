using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Organizations.Domain.OrganizationLegalProfiles;

public static class OrganizationLegalProfileErrors
{
    public static readonly Error EmptyId = Error.Validation(
        "OrganizationLegalProfiles.Id.Empty",
        "The organization legal profile identifier is required.");

    public static readonly Error EmptyOrganizationId = Error.Validation(
        "OrganizationLegalProfiles.OrganizationId.Empty",
        "The organization identifier is required.");

    public static readonly Error InvalidLegalForm = Error.Validation(
        "OrganizationLegalProfiles.LegalForm.Invalid",
        "The legal form is invalid.");

    public static readonly Error RegistrationNumberRequired = Error.Validation(
        "OrganizationLegalProfiles.RegistrationNumber.Required",
        "The registration number is required.");

    public static readonly Error InvalidRegisteredAddress = Error.Validation(
        "OrganizationLegalProfiles.RegisteredAddress.Invalid",
        "The registered address is invalid.");

    public static readonly Error CountryMismatch = Error.Validation(
        "OrganizationLegalProfiles.Country.Mismatch",
        "The registered address country must match the organization country.");

    public static readonly Error ArchivedProfileCannotBeChanged = Error.Conflict(
        "OrganizationLegalProfiles.Archived",
        "An archived organization legal profile cannot be changed.");

    public static readonly Error NotFound = Error.NotFound(
        "OrganizationLegalProfiles.NotFound",
        "errors.organizationLegalProfiles.notFound");

    public static readonly Error AlreadyExists = Error.Conflict(
        "OrganizationLegalProfiles.AlreadyExists",
        "The organization already has a legal profile.");

    public static readonly Error DuplicateRegistrationNumber = Error.Conflict(
        "OrganizationLegalProfiles.RegistrationNumber.Duplicate",
        "The registration number is already used by another organization in this country.");

    public static readonly Error OrganizationUnavailable = Error.Conflict(
        "OrganizationLegalProfiles.Organization.Unavailable",
        "The organization is closed or archived.");

    public static readonly Error CurrentUserRequired = Error.Validation(
        "OrganizationLegalProfiles.CurrentUser.Required",
        "An authenticated user is required.");

    public static readonly Error ConcurrentUpdate = Error.Conflict(
        "OrganizationLegalProfiles.ConcurrentUpdate",
        "The legal profile has been modified by another operation. Reload it and retry.");
}
