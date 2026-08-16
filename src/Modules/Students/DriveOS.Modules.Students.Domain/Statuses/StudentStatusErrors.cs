using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Students.Domain.Statuses;

public static class StudentStatusErrors
{
    public static readonly Error InvalidOwner = Error.Validation(
        "Students.Statuses.Owner.Invalid",
        "errors.students.statuses.owner.invalid"
    );
    public static readonly Error InvalidBlock = Error.Validation(
        "Students.Blocks.Invalid",
        "errors.students.blocks.invalid"
    );
    public static readonly Error BlockNotFound = Error.NotFound(
        "Students.Blocks.NotFound",
        "errors.students.blocks.notFound"
    );
    public static readonly Error BlockNotActive = Error.Conflict(
        "Students.Blocks.NotActive",
        "errors.students.blocks.notActive"
    );
    public static readonly Error ReasonRequired = Error.Validation(
        "Students.Blocks.Reason.Required",
        "errors.students.blocks.reason.required"
    );
    public static readonly Error OverridePeriodInvalid = Error.Validation(
        "Students.Blocks.OverridePeriod.Invalid",
        "errors.students.blocks.overridePeriod.invalid"
    );
}
