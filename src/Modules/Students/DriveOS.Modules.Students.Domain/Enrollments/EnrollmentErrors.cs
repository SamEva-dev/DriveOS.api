using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Students.Domain.Enrollments;

public static class EnrollmentErrors
{
    public static readonly Error InvalidId = Error.Validation(
        "Students.Enrollment.Id.Invalid",
        "errors.students.enrollment.id.invalid"
    );
    public static readonly Error InvalidOwner = Error.Validation(
        "Students.Enrollment.Owner.Invalid",
        "errors.students.enrollment.owner.invalid"
    );
    public static readonly Error TrainingCodeRequired = Error.Validation(
        "Students.Enrollment.TrainingCode.Required",
        "errors.students.enrollment.trainingCode.required"
    );
    public static readonly Error TrainingCodeTooLong = Error.Validation(
        "Students.Enrollment.TrainingCode.TooLong",
        "errors.students.enrollment.trainingCode.tooLong"
    );
    public static readonly Error InvalidDirectSource = Error.Validation(
        "Students.Enrollment.Source.Invalid",
        "errors.students.enrollment.source.invalid"
    );
    public static readonly Error InvalidIdempotencyKey = Error.Validation(
        "Students.Enrollment.IdempotencyKey.Invalid",
        "errors.students.enrollment.idempotencyKey.invalid"
    );
    public static readonly Error InvalidLocale = Error.Validation(
        "Students.Enrollment.Locale.Invalid",
        "errors.students.enrollment.locale.invalid"
    );
    public static readonly Error RequiredConsentsMissing = Error.Validation(
        "Students.Enrollment.Consents.Required",
        "errors.students.enrollment.consents.required"
    );
    public static readonly Error InvalidStatusTransition = Error.Conflict(
        "Students.Enrollment.StatusTransition.Invalid",
        "errors.students.enrollment.statusTransition.invalid"
    );
}
