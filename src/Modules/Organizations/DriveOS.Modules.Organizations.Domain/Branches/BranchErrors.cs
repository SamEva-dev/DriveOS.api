using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Organizations.Domain.Branches;

public static class BranchErrors
{
    public static readonly Error EmptyId =
        Error.Validation(
            "Branches.Id.Empty",
            "errors.branches.id.empty");

    public static readonly Error EmptyOrganizationId =
        Error.Validation(
            "Branches.OrganizationId.Empty",
            "errors.branches.organizationId.empty");

    public static readonly Error EmptyName =
        Error.Validation(
            "Branches.Name.Empty",
            "errors.branches.name.empty");

    public static Error NameTooLong(
        int maximumLength) =>
        Error.Validation(
            "Branches.Name.TooLong",
            "errors.branches.name.tooLong",
            new Dictionary<string, object?>
            {
                ["maximumLength"] =
                    maximumLength,
            });

    public static readonly Error EmptyCode =
        Error.Validation(
            "Branches.Code.Empty",
            "errors.branches.code.empty");

    public static Error CodeTooLong(
        int maximumLength) =>
        Error.Validation(
            "Branches.Code.TooLong",
            "errors.branches.code.tooLong",
            new Dictionary<string, object?>
            {
                ["maximumLength"] =
                    maximumLength,
            });

    public static readonly Error InvalidCode =
        Error.Validation(
            "Branches.Code.Invalid",
            "errors.branches.code.invalid");

    public static readonly Error InvalidAddress =
        Error.Validation(
            "Branches.Address.Invalid",
            "errors.branches.address.invalid");

    public static readonly Error InvalidCountryCode =
        Error.Validation(
            "Branches.CountryCode.Invalid",
            "errors.branches.countryCode.invalid");

    public static readonly Error InvalidTimeZone =
        Error.Validation(
            "Branches.TimeZone.Invalid",
            "errors.branches.timeZone.invalid");

    public static readonly Error InvalidBranchType =
        Error.Validation(
            "Branches.Type.Invalid",
            "errors.branches.type.invalid");

    public static readonly Error NotFound =
        Error.NotFound(
            "Branches.NotFound",
            "errors.branches.notFound");

    public static readonly Error OrganizationNotFound =
        Error.NotFound(
            "Branches.OrganizationNotFound",
            "errors.branches.organizationNotFound");

    public static readonly Error OrganizationUnavailable =
        Error.Conflict(
            "Branches.OrganizationUnavailable",
            "errors.branches.organizationUnavailable");

    public static readonly Error OrganizationMustBeActive =
        Error.Conflict(
            "Branches.Organization.MustBeActive",
            "errors.branches.organization.mustBeActive");

    public static readonly Error DuplicateName =
        Error.Conflict(
            "Branches.DuplicateName",
            "errors.branches.duplicateName");

    public static readonly Error DuplicateCode =
        Error.Conflict(
            "Branches.DuplicateCode",
            "errors.branches.duplicateCode");

    public static readonly Error ClosedBranchCannotBeModified =
        Error.Conflict(
            "Branches.Closed.CannotBeModified",
            "errors.branches.closed.cannotBeModified");

    public static readonly Error ClosedBranchCannotBePrimary =
        Error.Conflict(
            "Branches.Closed.CannotBePrimary",
            "errors.branches.closed.cannotBePrimary");

    public static Error InvalidStatusTransition(
        BranchStatus currentStatus,
        BranchStatus targetStatus) =>
        Error.Conflict(
            "Branches.Status.InvalidTransition",
            "errors.branches.status.invalidTransition",
            new Dictionary<string, object?>
            {
                ["currentStatus"] =
                    currentStatus.ToString(),

                ["targetStatus"] =
                    targetStatus.ToString(),
            });

    public static readonly Error EmptyManagerUserId =
    Error.Validation(
        "Branches.Manager.UserId.Empty",
        "errors.branches.manager.userId.empty");

    public static readonly Error EmptyAssignedByUserId =
        Error.Validation(
            "Branches.Manager.AssignedBy.Empty",
            "errors.branches.manager.assignedBy.empty");

    public static readonly Error ManagerEffectiveDateInvalid =
        Error.Validation(
            "Branches.Manager.EffectiveDate.Invalid",
            "errors.branches.manager.effectiveDate.invalid");

    public static readonly Error ClosedBranchCannotReceiveManager =
        Error.Conflict(
            "Branches.Manager.ClosedBranch",
            "errors.branches.manager.closedBranch");

    public static readonly Error ActiveManagerRequired =
        Error.Conflict(
            "Branches.Manager.Required",
            "errors.branches.manager.required");

    public static readonly Error CurrentManagerNotFound =
        Error.NotFound(
            "Branches.Manager.NotFound",
            "errors.branches.manager.notFound");
}