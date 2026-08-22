using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.ExamsCertification.Domain.Registrations.Preparation;

public static class ExamPreparationErrors
{
    public static readonly Error NotFound = Error.NotFound("Exams.Preparation.NotFound", "errors.exams.preparation.notFound");
    public static readonly Error InvalidIdentifier = Error.Validation("Exams.Preparation.InvalidIdentifier", "errors.exams.preparation.invalidIdentifier");
    public static readonly Error RegistrationNotConfirmed = Error.Conflict("Exams.Preparation.RegistrationNotConfirmed", "errors.exams.preparation.registrationNotConfirmed");
    public static readonly Error ConvocationMissing = Error.Conflict("Exams.Preparation.ConvocationMissing", "errors.exams.preparation.convocationMissing");
    public static readonly Error ResourceAssignmentMissing = Error.Conflict("Exams.Preparation.ResourceAssignmentMissing", "errors.exams.preparation.resourceAssignmentMissing");
    public static readonly Error OperationConflict = Error.Conflict("Exams.Preparation.OperationConflict", "errors.exams.preparation.operationConflict");
    public static readonly Error InvalidSnapshot = Error.Validation("Exams.Preparation.InvalidSnapshot", "errors.exams.preparation.invalidSnapshot");
    public static readonly Error DuplicateCheckCode = Error.Validation("Exams.Preparation.DuplicateCheckCode", "errors.exams.preparation.duplicateCheckCode");
    public static readonly Error NotReadyForConfirmation = Error.Conflict("Exams.Preparation.NotReadyForConfirmation", "errors.exams.preparation.notReadyForConfirmation");
}
