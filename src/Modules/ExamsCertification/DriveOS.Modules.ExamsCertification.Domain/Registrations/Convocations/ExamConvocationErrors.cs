using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.ExamsCertification.Domain.Registrations.Convocations;

public static class ExamConvocationErrors
{
    public static readonly Error NotFound = Error.NotFound("Exams.Convocation.NotFound", "errors.exams.convocation.notFound");
    public static readonly Error RegistrationNotConfirmed = Error.Conflict("Exams.Convocation.RegistrationNotConfirmed", "errors.exams.convocation.registrationNotConfirmed");
    public static readonly Error InvalidIdentifier = Error.Validation("Exams.Convocation.InvalidIdentifier", "errors.exams.convocation.invalidIdentifier");
    public static readonly Error InvalidPeriod = Error.Validation("Exams.Convocation.InvalidPeriod", "errors.exams.convocation.invalidPeriod");
    public static readonly Error CenterRequired = Error.Validation("Exams.Convocation.CenterRequired", "errors.exams.convocation.centerRequired");
    public static readonly Error ProviderRequired = Error.Validation("Exams.Convocation.ProviderRequired", "errors.exams.convocation.providerRequired");
    public static readonly Error InvalidOperation = Error.Validation("Exams.Convocation.InvalidOperation", "errors.exams.convocation.invalidOperation");
    public static readonly Error OperationConflict = Error.Conflict("Exams.Convocation.OperationConflict", "errors.exams.convocation.operationConflict");
    public static readonly Error DeliveryAlreadyAcknowledged = Error.Conflict("Exams.Convocation.DeliveryAlreadyAcknowledged", "errors.exams.convocation.deliveryAlreadyAcknowledged");
    public static readonly Error MustBeDeliveredFirst = Error.Conflict("Exams.Convocation.MustBeDeliveredFirst", "errors.exams.convocation.mustBeDeliveredFirst");
    public static readonly Error InvalidInternalMeetingTime = Error.Validation("Exams.Convocation.InvalidInternalMeetingTime", "errors.exams.convocation.invalidInternalMeetingTime");
}
