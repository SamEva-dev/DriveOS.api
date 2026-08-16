using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Students.Domain.Checklists;

public static class EnrollmentChecklistErrors
{
    public static readonly Error InvalidOwner = Error.Validation(
        "Students.Checklist.Owner.Invalid",
        "errors.students.checklist.owner.invalid"
    );
    public static readonly Error InvalidItem = Error.Validation(
        "Students.Checklist.Item.Invalid",
        "errors.students.checklist.item.invalid"
    );
    public static readonly Error ItemNotFound = Error.NotFound(
        "Students.Checklist.Item.NotFound",
        "errors.students.checklist.item.notFound"
    );
    public static readonly Error ReasonRequired = Error.Validation(
        "Students.Checklist.Reason.Required",
        "errors.students.checklist.reason.required"
    );
    public static readonly Error BlockingItemsIncomplete = Error.Conflict(
        "Students.Checklist.BlockingItems.Incomplete",
        "errors.students.checklist.blockingItems.incomplete"
    );
    public static readonly Error AlreadyActive = Error.Conflict(
        "Students.Checklist.Enrollment.AlreadyActive",
        "errors.students.checklist.enrollment.alreadyActive"
    );
}
