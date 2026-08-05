using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Organizations.Domain.OrganizationRepresentatives;

public static class OrganizationRepresentativeErrors
{
    public static readonly Error EmptyId = Error.Validation(
        "OrganizationRepresentatives.Id.Empty",
        "errors.organizationRepresentative.id.empty");

    public static readonly Error EmptyOrganizationId = Error.Validation(
        "OrganizationRepresentatives.OrganizationId.Empty",
        "errors.organizationRepresentative.organizationId.empty");

    public static readonly Error EmptyPersonId = Error.Validation(
        "OrganizationRepresentatives.PersonId.Empty",
        "errors.organizationRepresentative.personId.empty");

    public static readonly Error InvalidType = Error.Validation(
        "OrganizationRepresentatives.Type.Invalid",
        "errors.organizationRepresentative.type.invalid");

    public static readonly Error InvalidEffectivePeriod = Error.Validation(
        "OrganizationRepresentatives.EffectivePeriod.Invalid",
        "errors.organizationRepresentative.effectivePeriod.invalid");

    public static readonly Error AuthorityScopeRequired = Error.Validation(
        "OrganizationRepresentatives.AuthorityScope.Required",
        "errors.organizationRepresentative.authorityScope.required");

    public static Error AuthorityScopeTooLong(int maximumLength) => Error.Validation(
        "OrganizationRepresentatives.AuthorityScope.TooLong",
        "errors.organizationRepresentative.authorityScope.tooLong",
        new Dictionary<string, object?> { ["maximumLength"] = maximumLength });

    public static readonly Error InvalidStatusTransition = Error.Conflict(
        "OrganizationRepresentatives.Status.InvalidTransition",
        "errors.organizationRepresentative.status.invalidTransition");

    public static readonly Error LastActiveOwnerCannotBeEnded = Error.Conflict(
        "OrganizationRepresentatives.Owner.LastActiveCannotBeEnded",
        "errors.organizationRepresentative.owner.lastActiveCannotBeEnded");

    public static readonly Error PrimaryOwnerMustBeOwner = Error.Validation(
        "OrganizationRepresentatives.PrimaryOwner.MustBeOwner",
        "errors.organizationRepresentative.primaryOwner.mustBeOwner");

    public static readonly Error DuplicateActiveRepresentation = Error.Conflict(
        "OrganizationRepresentatives.ActiveRepresentation.Duplicate",
        "errors.organizationRepresentative.activeRepresentation.duplicate");

    public static readonly Error NotFound = Error.NotFound(
        "OrganizationRepresentatives.NotFound",
        "errors.organizationRepresentative.notFound");
    public static readonly Error CurrentUserRequired = Error.Unauthorized(
        "OrganizationRepresentatives.CurrentUser.Required",
        "errors.organizationRepresentative.currentUser.required");

    public static readonly Error OrganizationUnavailable = Error.Conflict(
        "OrganizationRepresentatives.Organization.Unavailable",
        "errors.organizationRepresentative.organization.unavailable");

    public static readonly Error ConcurrentUpdate = Error.Conflict(
        "OrganizationRepresentatives.ConcurrentUpdate",
        "errors.organizationRepresentative.concurrentUpdate");

}
