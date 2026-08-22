using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.ExamsCertification.Domain.Registrations.Attempts;

public static class ExamAttemptErrors
{
    public static readonly Error NotFound = Error.NotFound("Exams.Attempt.NotFound", "errors.exams.attempt.notFound");
    public static readonly Error InvalidIdentifier = Error.Validation("Exams.Attempt.InvalidIdentifier", "errors.exams.attempt.invalidIdentifier");
    public static readonly Error InvalidSnapshot = Error.Validation("Exams.Attempt.InvalidSnapshot", "errors.exams.attempt.invalidSnapshot");
    public static readonly Error RegistrationNotConfirmed = Error.Conflict("Exams.Attempt.RegistrationNotConfirmed", "errors.exams.attempt.registrationNotConfirmed");
    public static readonly Error PreparationNotConfirmed = Error.Conflict("Exams.Attempt.PreparationNotConfirmed", "errors.exams.attempt.preparationNotConfirmed");
    public static readonly Error PreparationChanged = Error.Conflict("Exams.Attempt.PreparationChanged", "errors.exams.attempt.preparationChanged");
    public static readonly Error ResourceAssignmentMissing = Error.Conflict("Exams.Attempt.ResourceAssignmentMissing", "errors.exams.attempt.resourceAssignmentMissing");
    public static readonly Error OperationalPlanMissing = Error.Conflict("Exams.Attempt.OperationalPlanMissing", "errors.exams.attempt.operationalPlanMissing");
    public static readonly Error OperationalPlanNotReady = Error.Conflict("Exams.Attempt.OperationalPlanNotReady", "errors.exams.attempt.operationalPlanNotReady");
    public static readonly Error ConvocationVersionMismatch = Error.Conflict("Exams.Attempt.ConvocationVersionMismatch", "errors.exams.attempt.convocationVersionMismatch");
    public static readonly Error BookingNotConfirmed = Error.Conflict("Exams.Attempt.BookingNotConfirmed", "errors.exams.attempt.bookingNotConfirmed");
    public static readonly Error AlreadyExists = Error.Conflict("Exams.Attempt.AlreadyExists", "errors.exams.attempt.alreadyExists");
    public static readonly Error InvalidTransition = Error.Conflict("Exams.Attempt.InvalidTransition", "errors.exams.attempt.invalidTransition");
    public static readonly Error OperationConflict = Error.Conflict("Exams.Attempt.OperationConflict", "errors.exams.attempt.operationConflict");
    public static readonly Error InvalidOperation = Error.Validation("Exams.Attempt.InvalidOperation", "errors.exams.attempt.invalidOperation");
    public static readonly Error IncidentDetailsRequired = Error.Validation("Exams.Attempt.IncidentDetailsRequired", "errors.exams.attempt.incidentDetailsRequired");
    public static readonly Error NoteRequired = Error.Validation("Exams.Attempt.NoteRequired", "errors.exams.attempt.noteRequired");
    public static readonly Error InvalidLocation = Error.Validation("Exams.Attempt.InvalidLocation", "errors.exams.attempt.invalidLocation");
    public static readonly Error ResourceChangeRequired = Error.Validation("Exams.Attempt.ResourceChangeRequired", "errors.exams.attempt.resourceChangeRequired");
    public static readonly Error ResourceAssignmentChanged = Error.Conflict("Exams.Attempt.ResourceAssignmentChanged", "errors.exams.attempt.resourceAssignmentChanged");
    public static readonly Error ReasonRequired = Error.Validation("Exams.Attempt.ReasonRequired", "errors.exams.attempt.reasonRequired");
}
