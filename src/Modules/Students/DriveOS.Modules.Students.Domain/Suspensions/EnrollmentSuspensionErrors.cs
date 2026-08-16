using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Students.Domain.Suspensions;

public static class EnrollmentSuspensionErrors
{
    public static readonly Error InvalidOwner = Error.Validation(
        "Students.Suspension.InvalidOwner",
        "errors.students.suspension.invalidOwner"
    );
    public static readonly Error InvalidRequest = Error.Validation(
        "Students.Suspension.InvalidRequest",
        "errors.students.suspension.invalidRequest"
    );
    public static readonly Error SuspensionNotFound = Error.NotFound(
        "Students.Suspension.NotFound",
        "errors.students.suspension.notFound"
    );
    public static readonly Error ActiveSuspensionExists = Error.Conflict(
        "Students.Suspension.ActiveExists",
        "errors.students.suspension.activeExists"
    );
    public static readonly Error FutureBookingsUntreated = Error.Conflict(
        "Students.Suspension.FutureBookingsUntreated",
        "errors.students.suspension.futureBookingsUntreated"
    );
    public static readonly Error InvalidTransition = Error.Conflict(
        "Students.Suspension.InvalidTransition",
        "errors.students.suspension.invalidTransition"
    );
    public static readonly Error ReactivationChecksIncomplete = Error.Conflict(
        "Students.Reactivation.ChecksIncomplete",
        "errors.students.reactivation.checksIncomplete"
    );
    public static readonly Error ReactivationAlreadyExists = Error.Conflict(
        "Students.Reactivation.AlreadyExists",
        "errors.students.reactivation.alreadyExists"
    );
    public static readonly Error ReactivationNotFound = Error.NotFound(
        "Students.Reactivation.NotFound",
        "errors.students.reactivation.notFound"
    );
}
