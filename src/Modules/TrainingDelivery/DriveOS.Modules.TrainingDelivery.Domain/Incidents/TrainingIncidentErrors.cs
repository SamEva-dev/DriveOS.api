using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.TrainingDelivery.Domain.Incidents;

public static class TrainingIncidentErrors
{
    public static readonly Error NotFound = Error.NotFound("TrainingDelivery.Incident.NotFound", "errors.trainingDelivery.incident.notFound");
    public static readonly Error Invalid = Error.Validation("TrainingDelivery.Incident.Invalid", "errors.trainingDelivery.incident.invalid");
    public static readonly Error InvalidSessionStatus = Error.Conflict("TrainingDelivery.Incident.SessionStatus.Invalid", "errors.trainingDelivery.incident.sessionStatus.invalid");
    public static readonly Error OccurredAtInvalid = Error.Validation("TrainingDelivery.Incident.OccurredAt.Invalid", "errors.trainingDelivery.incident.occurredAt.invalid");
    public static readonly Error TextTooLong = Error.Validation("TrainingDelivery.Incident.Text.TooLong", "errors.trainingDelivery.incident.text.tooLong");
    public static readonly Error ParticipantInvalid = Error.Validation("TrainingDelivery.Incident.Participant.Invalid", "errors.trainingDelivery.incident.participant.invalid");
    public static readonly Error EvidenceInvalid = Error.Validation("TrainingDelivery.Incident.Evidence.Invalid", "errors.trainingDelivery.incident.evidence.invalid");
    public static readonly Error OperationConflict = Error.Conflict("TrainingDelivery.Incident.Operation.Conflict", "errors.trainingDelivery.incident.operation.conflict");
    public static readonly Error AlreadyResolved = Error.Conflict("TrainingDelivery.Incident.AlreadyResolved", "errors.trainingDelivery.incident.alreadyResolved");
    public static readonly Error AlreadyClosed = Error.Conflict("TrainingDelivery.Incident.AlreadyClosed", "errors.trainingDelivery.incident.alreadyClosed");
    public static readonly Error CriticalMustBeEscalated = Error.Conflict("TrainingDelivery.Incident.Critical.MustBeEscalated", "errors.trainingDelivery.incident.critical.mustBeEscalated");
    public static readonly Error ResolutionRequired = Error.Validation("TrainingDelivery.Incident.Resolution.Required", "errors.trainingDelivery.incident.resolution.required");
}
