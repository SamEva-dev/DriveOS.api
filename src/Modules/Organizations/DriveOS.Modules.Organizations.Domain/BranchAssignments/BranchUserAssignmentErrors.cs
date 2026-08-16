using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Organizations.Domain.BranchAssignments;

public static class BranchUserAssignmentErrors
{
    public static readonly Error EmptyId = Error.Validation(
        "BranchAssignments.Id.Empty",
        "errors.branchAssignments.id.empty"
    );

    public static readonly Error EmptyOrganizationId = Error.Validation(
        "BranchAssignments.OrganizationId.Empty",
        "errors.branchAssignments.organizationId.empty"
    );

    public static readonly Error EmptyBranchId = Error.Validation(
        "BranchAssignments.BranchId.Empty",
        "errors.branchAssignments.branchId.empty"
    );

    public static readonly Error EmptyUserId = Error.Validation(
        "BranchAssignments.UserId.Empty",
        "errors.branchAssignments.userId.empty"
    );

    public static readonly Error EmptyCreatedByUserId = Error.Validation(
        "BranchAssignments.CreatedBy.Empty",
        "errors.branchAssignments.createdBy.empty"
    );

    public static readonly Error EmptyChangedByUserId = Error.Validation(
        "BranchAssignments.ChangedBy.Empty",
        "errors.branchAssignments.changedBy.empty"
    );

    public static readonly Error InvalidRole = Error.Validation(
        "BranchAssignments.Role.Invalid",
        "errors.branchAssignments.role.invalid"
    );

    public static readonly Error InvalidType = Error.Validation(
        "BranchAssignments.Type.Invalid",
        "errors.branchAssignments.type.invalid"
    );

    public static readonly Error InvalidStartDate = Error.Validation(
        "BranchAssignments.StartDate.Invalid",
        "errors.branchAssignments.startDate.invalid"
    );

    public static readonly Error InvalidEndDate = Error.Validation(
        "BranchAssignments.EndDate.Invalid",
        "errors.branchAssignments.endDate.invalid"
    );

    public static readonly Error EmptyReason = Error.Validation(
        "BranchAssignments.Reason.Empty",
        "errors.branchAssignments.reason.empty"
    );

    public static readonly Error ReasonTooLong = Error.Validation(
        "BranchAssignments.Reason.TooLong",
        "errors.branchAssignments.reason.tooLong"
    );

    public static readonly Error AlreadySuspended = Error.Conflict(
        "BranchAssignments.AlreadySuspended",
        "errors.branchAssignments.alreadySuspended"
    );

    public static readonly Error NotSuspended = Error.Conflict(
        "BranchAssignments.NotSuspended",
        "errors.branchAssignments.notSuspended"
    );

    public static readonly Error AlreadyEnded = Error.Conflict(
        "BranchAssignments.AlreadyEnded",
        "errors.branchAssignments.alreadyEnded"
    );

    public static readonly Error CannotModifyEnded = Error.Conflict(
        "BranchAssignments.Ended",
        "errors.branchAssignments.ended"
    );

    public static readonly Error DuplicateActiveAssignment = Error.Conflict(
        "BranchAssignments.Duplicate",
        "errors.branchAssignments.duplicate"
    );

    public static readonly Error PrimaryAssignmentAlreadyExists = Error.Conflict(
        "BranchAssignments.PrimaryAlreadyExists",
        "errors.branchAssignments.primaryAlreadyExists"
    );

    public static readonly Error OrganizationNotFound = Error.NotFound(
        "BranchAssignments.OrganizationNotFound",
        "errors.branchAssignments.organizationNotFound"
    );

    public static readonly Error BranchNotFound = Error.NotFound(
        "BranchAssignments.BranchNotFound",
        "errors.branchAssignments.branchNotFound"
    );

    public static readonly Error NotFound = Error.NotFound(
        "BranchAssignments.NotFound",
        "errors.branchAssignments.notFound"
    );

    public static readonly Error ClosedBranch = Error.Conflict(
        "BranchAssignments.BranchClosed",
        "errors.branchAssignments.branchClosed"
    );

    public static Error InvalidStatus() =>
        Error.Validation(
            "BranchAssignments.Status.Invalid",
            "errors.branchAssignments.status.invalid"
        );
}
