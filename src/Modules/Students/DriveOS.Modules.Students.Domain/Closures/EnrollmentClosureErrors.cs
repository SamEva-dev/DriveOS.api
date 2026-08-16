using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Students.Domain.Closures;

public static class EnrollmentClosureErrors
{
    public static readonly Error InvalidOwner = new(
        "Students.Closure.InvalidOwner",
        "students.closure.invalidOwner",
        ErrorType.Validation
    );
    public static readonly Error InvalidReason = new(
        "Students.Closure.InvalidReason",
        "students.closure.invalidReason",
        ErrorType.Validation
    );
    public static readonly Error InvalidDate = new(
        "Students.Closure.InvalidDate",
        "students.closure.invalidDate",
        ErrorType.Validation
    );
    public static readonly Error InvalidChecks = new(
        "Students.Closure.InvalidChecks",
        "students.closure.invalidChecks",
        ErrorType.Validation
    );
    public static readonly Error PreconditionsNotResolved = new(
        "Students.Closure.PreconditionsNotResolved",
        "students.closure.preconditionsNotResolved",
        ErrorType.Conflict
    );
    public static readonly Error NotFound = new(
        "Students.Closure.NotFound",
        "students.closure.notFound",
        ErrorType.NotFound
    );
    public static readonly Error AlreadyExists = new(
        "Students.Closure.AlreadyExists",
        "students.closure.alreadyExists",
        ErrorType.Conflict
    );
    public static readonly Error InvalidTransition = new(
        "Students.Closure.InvalidTransition",
        "students.closure.invalidTransition",
        ErrorType.Conflict
    );
    public static readonly Error RetentionRequired = new(
        "Students.Closure.RetentionRequired",
        "students.closure.retentionRequired",
        ErrorType.Validation
    );
    public static readonly Error ReopenJustificationRequired = new(
        "Students.Closure.ReopenJustificationRequired",
        "students.closure.reopenJustificationRequired",
        ErrorType.Validation
    );
    public static readonly Error ActiveEnrollmentNotFound = new(
        "Students.Closure.ActiveEnrollmentNotFound",
        "students.closure.activeEnrollmentNotFound",
        ErrorType.NotFound
    );
    public static readonly Error StatusBoardNotFound = new(
        "Students.Closure.StatusBoardNotFound",
        "students.closure.statusBoardNotFound",
        ErrorType.NotFound
    );
}
